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

            var idFolder = folder.GetChildDir(id);
            var oldImg = idFolder.GetChild(id + ".regression.jpg");
            var newImg = idFolder.GetChild(id + ".jpg");
            var backup = idFolder.GetChild(id + ".jpg.backup");

            var configFile = idFolder.GetChild(configFileName);
            Config config = this.config;
            if (configFile.IsNotNullAndExists()) {
                config = configFile.LoadAs<Config>();
            } else {
                idFolder.CreateV2().GetChild(configFileName + ".example").SaveAsJson(config, asPrettyString: true);
            }

            yield return new WaitForEndOfFrame();
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture(config.screenshotUpscaleFactor);
            // Texture2D screenShot = Camera.allCameras.CaptureScreenshot(); // Does not capture UI 

            if (newImg.Exists) { newImg.CopyToV2(backup, replaceExisting: false); }

            screenShot.SaveToJpgFile(newImg, config.screenshotQuality);
            screenShot.Destroy();

            var diffImg = CalculateDiffImage(oldImg, newImg, config.maxAllowedDiff);
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

        private static FileEntry CalculateDiffImage(FileEntry oldImg, FileEntry newImg, double maxAllowedDiff) {
            if (oldImg.Exists) {
#if ENABLE_IMAGE_MAGICK
                using (ImageMagick.MagickImage original = new ImageMagick.MagickImage()) {
                    original.LoadFromFileEntry(oldImg);
                    var diff = original.Compare(newImg, maxAllowedDiff);
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