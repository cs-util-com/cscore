using ImageMagick;
using System.Collections;
using UnityEngine;
using Zio;

namespace com.csutil {

    public class PersistedImageRegression {

        public bool openExternallyOnAssertFail = true;
        public bool throwAssertException;
        public DirectoryEntry folder;

        public PersistedImageRegression(DirectoryEntry folderToStoreImagesIn) { folder = folderToStoreImagesIn; }

        public IEnumerator AssertEqualToPersisted(string id) {

            var idFolder = folder.GetChildDir(id);
            var oldImg = idFolder.GetChild(id + "_regression.jpg");
            var newImg = idFolder.GetChild(id + ".jpg");

            //Camera c = Camera.main;
            //Texture2D screenShot = c.CaptureScreenshot(400); // Would not capture UI?
            yield return new WaitForEndOfFrame();
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            screenShot.SaveToFile(newImg, quality: 100);
            screenShot.Destroy();

            if (oldImg.Exists) {
                using (MagickImage original = new MagickImage()) {
                    original.LoadFromFileEntry(oldImg);
                    var diff = original.Compare(newImg);
                    if (diff != null) {
                        var e = Log.e($"Visual difference to previous version detected! To approve an allowed visual change, delete '{oldImg.Name}'");
                        if (throwAssertException) { throw e; }
                        if (openExternallyOnAssertFail) {
                            diff.Parent.OpenInExternalApp();
                            diff.OpenInExternalApp();
                        }
                    } else {
                        oldImg.DeleteV2();
                        newImg.Rename(oldImg.Name, out newImg);
                    }
                }
            } else {
                newImg.Rename(oldImg.Name, out newImg);
            }

        }

    }

}