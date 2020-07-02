using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;
using Zio;

namespace com.csutil {

    public class AssertVisually {

        public class Config {
            public bool throwAssertException = false;
            public bool logAsError = true;
            public bool logAsWarning = true;
            public bool openExternallyOnAssertFail = true;
            public int screenshotQuality = 70;
            public int screenshotUpscaleFactor = 1;
            public double maxAllowedDiff = 0.0005;
            public string customErrorMessage = "";
            /// <summary> Any of the ImageMagick.ErrorMetric enum entry names </summary>
            public string errorMetric = "MeanSquared";
            public float minReqMsWithNoVisualChange = 1000;
            public float msBetweenVisualChangeDetectCaptures = 200;
            public long maxMsToWaitForNoVisualChange = 5000;
        }

        public Config config = new Config();
        public DirectoryEntry folder;
        public string configFileName = "config.txt";

        public static Task AssertNoVisualChange(string id, object caller = null) {
            var i = IoC.inject.Get<AssertVisually>(caller);
            if (i == null) {
                Log.d($"No visual regression instance injected, AssertVisually.SetupDefaultSingletonInDebugMode()" +
                    $" not called? Will skip AssertNoVisualChange check for '{id}'");
                return Task.FromResult(false);
            }
            return i.AssertNoVisualChange(id);
        }

        /// <summary> This will if building in debug mode set up the AssertVisually system and in production do nothing </summary>
        [Conditional("DEBUG")]
        public static void SetupDefaultSingletonInDebugMode() {
            IoC.inject.GetOrAddSingleton(null, () => new AssertVisually(EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("Visual_Assertions")));
        }

        public AssertVisually(DirectoryEntry folderToStoreImagesIn) { folder = folderToStoreImagesIn; }

        public Task AssertNoVisualChange(string id) {
            StackTrace stacktrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
            return MainThread.instance.StartCoroutineAsTask(AssertNoVisualRegressionCoroutine(id, stacktrace));
        }

        private IEnumerator AssertNoVisualRegressionCoroutine(string id, StackTrace stacktrace) {
            id = EnvironmentV2.SanatizeToFileName(id);
            if (id.IsNullOrEmpty()) { throw new ArgumentNullException("Invalid ID passed"); }

            var idFolder = GetFolderFor(id);
            var oldImg = idFolder.GetChild(id + ".regression.jpg");
            var newImg = idFolder.GetChild(id + ".jpg");
            var backup = idFolder.GetChild(id + ".jpg.backup");

            Config config = LoadConfigFor(id);

            yield return new WaitForEndOfFrame();
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture(config.screenshotUpscaleFactor);
            // Texture2D screenShot = Camera.allCameras.CaptureScreenshot(); // Does not capture UI 

            if (newImg.Exists) { newImg.CopyToV2(backup, replaceExisting: false); }
            screenShot.SaveToJpgFile(newImg, config.screenshotQuality);
            screenShot.Destroy();

            var diffImg = CalculateDiffImage(oldImg, newImg, config.maxAllowedDiff, config.errorMetric);
            if (diffImg != null) {
                var e = $"Visual diff to previous '{id}' detected! To approve an allowed visual change, delete '{oldImg.Name}'";
                if (!config.customErrorMessage.IsNullOrEmpty()) { e = config.customErrorMessage + "/n" + e; }
                var exeption = new Error(e, stacktrace);
                if (config.throwAssertException) {
                    throw exeption;
                } else if (config.logAsError) {
                    Log.e(exeption);
                } else if (config.logAsWarning) {
                    Log.w(e);
                }
                if (config.openExternallyOnAssertFail) {
                    diffImg.Parent.OpenInExternalApp();
                    diffImg.OpenInExternalApp();
                }
            } else { // No difference between oldImg and newImg
                // To prevent git from detecting invalid file changes: 
                if (backup.Exists) { // If there is a backup of newImg..
                    newImg.DeleteV2(); // Remove the newly generated version ..
                    backup.Rename(newImg.Name, out FileEntry _); // and replace it with the backup
                }
            }
            backup.DeleteV2(); // If a backup file was created during the process delete it
            AssertV2.IsTrue(newImg.Exists, "newImg did not exist after AssertNoVisualChange done");

        }

        private Config LoadConfigFor(string id) {
            var configFile = GetFolderFor(id).GetChild(configFileName);
            Config config = this.config;
            if (configFile.IsNotNullAndExists()) {
                config = configFile.LoadAs<Config>();
            } else {
                GetFolderFor(id).GetChild(configFileName + ".example").SaveAsJson(config, asPrettyString: true);
            }
            return config;
        }

        private DirectoryEntry GetFolderFor(string id) { return folder.GetChildDir(id).CreateV2(); }

        public Task WaitForNoVisualChangeInScene(Config config = null) {
            if (config == null) { config = this.config; }
            StackTrace stacktrace = new StackTrace(skipFrames: 2, fNeedFileInfo: true);
            return MainThread.instance.StartCoroutineAsTask(WaitForNoVisualChangeCoroutine(config));
        }

        private IEnumerator WaitForNoVisualChangeCoroutine(Config config) {
#if ENABLE_IMAGE_MAGICK
            var consecutiveNoDiffNeeded = config.minReqMsWithNoVisualChange / config.msBetweenVisualChangeDetectCaptures;
            var noDiffCounter = 0;
            var timer = Stopwatch.StartNew();
            do {
                yield return new WaitForEndOfFrame();
                Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture(config.screenshotUpscaleFactor);
                yield return new WaitForSeconds(config.msBetweenVisualChangeDetectCaptures / 1000f);
                yield return new WaitForEndOfFrame();
                Texture2D screenShot2 = ScreenCapture.CaptureScreenshotAsTexture(config.screenshotUpscaleFactor);
                var visualDiffDetected = AreVisuallyDifferent(screenShot, screenShot2, config);
                if (!visualDiffDetected) { noDiffCounter++; } else { noDiffCounter = 0; }
                screenShot.Destroy();
                screenShot2.Destroy();
                if (timer.ElapsedMilliseconds > config.maxMsToWaitForNoVisualChange) {
                    throw new TimeoutException($"WaitForNoVisualChange not done after {config.maxMsToWaitForNoVisualChange}ms");
                }
            } while (noDiffCounter < consecutiveNoDiffNeeded);
#else
            Log.w("ENABLE_IMAGE_MAGICK define not active, see instructions 'readme Image Magick Unity Installation Instructions.txt'");
            yield return null;
#endif
        }

#if ENABLE_IMAGE_MAGICK
        private bool AreVisuallyDifferent(Texture2D img1, Texture2D img2, Config config) {
            using (ImageMagick.MagickImage s1 = new ImageMagick.MagickImage()) {
                s1.Read(img1.EncodeToJPG(config.screenshotQuality));
                using (ImageMagick.MagickImage s2 = new ImageMagick.MagickImage()) {
                    s2.Read(img2.EncodeToJPG(config.screenshotQuality));
                    var errorMetric = EnumUtil.Parse<ImageMagick.ErrorMetric>(config.errorMetric);
                    var diffValue = s1.CompareV2(s2, errorMetric, out ImageMagick.MagickImage diffImg);
                    if (diffValue < config.maxAllowedDiff) { return false; }
                    diffImg.Dispose();
                }
            }
            return true;
        }
#endif

        private static FileEntry CalculateDiffImage(FileEntry oldImg, FileEntry newImg, double maxAllowedDiff, string errorMetric) {
            if (oldImg.Exists) {
#if ENABLE_IMAGE_MAGICK
                using (ImageMagick.MagickImage original = new ImageMagick.MagickImage()) {
                    original.LoadFromFileEntry(oldImg);
                    ImageMagick.ErrorMetric eM = EnumUtil.Parse<ImageMagick.ErrorMetric>(errorMetric);
                    var diff = original.Compare(newImg, eM, maxAllowedDiff);
                    if (diff != null) {
                        return diff;
                    } else {
                        newImg.CopyToV2(oldImg, replaceExisting: true);
                    }
                }
#else
                Log.w("ENABLE_IMAGE_MAGICK define not active, see instructions 'readme Image Magick Unity Installation Instructions.txt'");
#endif
            } else {
                newImg.CopyToV2(oldImg, replaceExisting: false);
            }
            return null;
        }

    }

}