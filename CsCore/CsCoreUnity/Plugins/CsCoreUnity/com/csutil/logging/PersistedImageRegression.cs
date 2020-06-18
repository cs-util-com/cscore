using ImageMagick;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Zio;

namespace com.csutil {

    public class AssertVisually {

        public bool openExternallyOnAssertFail = true;
        public bool throwAssertException = false;
        public int screenshotQuality = 70;
        public double maxAllowedDiff = 0.0005;
        public DirectoryEntry folder;

        public static Task AssertNoVisualChange(string id, object caller = null) {
            var i = IoC.inject.Get<AssertVisually>(caller);
            if (i == null) {
                Log.w($"No visual regression instance injected, AssertVisually.SetupDefaultAssertVisuallySingleton()" +
                    $" not called? Will skip AssertNoVisualChange check for '{id}'");
                return Task.FromResult(false);
            }
            return i.AssertNoVisualChange(id);
        }

        public static void SetupDefaultAssertVisuallySingleton() {
            IoC.inject.GetOrAddSingleton(null, () => new AssertVisually(EnvironmentV2.instance.GetCurrentDirectory().GetChildDir("Visual_Assertions")));
        }

        public AssertVisually(DirectoryEntry folderToStoreImagesIn) { folder = folderToStoreImagesIn; }

        public Task AssertNoVisualChange(string id) {
            return MainThread.instance.StartCoroutineAsTask(AssertNoVisualRegressionCoroutine(id));
        }

        private IEnumerator AssertNoVisualRegressionCoroutine(string id) {

            var idFolder = folder.GetChildDir(id);
            var oldImg = idFolder.GetChild(id + ".regression.jpg");
            var newImg = idFolder.GetChild(id + ".jpg");

            yield return new WaitForEndOfFrame();
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            //Camera c = Camera.main;
            //Texture2D screenShot = c.CaptureScreenshot(400); // Would not capture UI?

            screenShot.SaveToFile(newImg, screenshotQuality);
            screenShot.Destroy();

            var diffImg = CalculateDiffImage(oldImg, newImg, maxAllowedDiff);
            if (diffImg != null) {
                var e = Log.e($"Visual diff to previous '{id}' detected! To approve an allowed visual change, delete '{oldImg.Name}'");
                if (throwAssertException) { throw e; }
                if (openExternallyOnAssertFail) {
                    diffImg.Parent.OpenInExternalApp();
                    diffImg.OpenInExternalApp();
                }
            }

        }

        private static FileEntry CalculateDiffImage(FileEntry oldImg, FileEntry newImg, double maxAllowedDiff) {
            if (oldImg.Exists) {
                using (MagickImage original = new MagickImage()) {
                    original.LoadFromFileEntry(oldImg);
                    var diff = original.Compare(newImg, maxAllowedDiff);
                    if (diff != null) {
                        return diff;
                    } else {
                        oldImg.DeleteV2();
                        newImg.Rename(oldImg.Name, out newImg);
                    }
                }
            } else {
                newImg.CopyToV2(oldImg, false);
            }
            return null;
        }

    }

}