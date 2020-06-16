using ImageMagick;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zio;

namespace com.csutil {

    public class PersistedImageRegression {

        private DirectoryEntry folder;


        public PersistedImageRegression(DirectoryEntry folderToStoreImagesIn) { folder = folderToStoreImagesIn; }

        public void AssertEqualToPersisted(string id) {


            var oldImg = folder.GetChild(id + "_old.jpg");
            var newImg = folder.GetChild(id + ".jpg");

            //Camera c = Camera.main;
            //Texture2D screenShot = c.CaptureScreenshot(400); // Would not capture UI?
            Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
            screenShot.SaveToFile(newImg);
            screenShot.Destroy();

            if (oldImg.Exists) {
                using (MagickImage original = new MagickImage()) {
                    original.LoadFromFileEntry(oldImg);
                    var diff = original.Compare(newImg);
                    if (diff != null) {
                        Log.e("Diff detected!");
                        diff.Parent.OpenInExternalApp();
                        diff.OpenInExternalApp();
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