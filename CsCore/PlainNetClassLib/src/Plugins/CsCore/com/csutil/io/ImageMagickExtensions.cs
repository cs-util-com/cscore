#if ENABLE_IMAGE_MAGICK

using System;
using System.IO;
using Zio;
using ImageMagick;

namespace com.csutil {

    public static class MagickImageExtensions {

        public static void SaveToFileEntry(this MagickImage self, FileEntry f) {
            using (Stream outStream = f.OpenOrCreateForWrite()) { self.Write(outStream); }
        }

        public static void LoadFromFileEntry(this MagickImage self, FileEntry f) {
            using (Stream inputStream = f.OpenForRead()) { self.Read(inputStream); }
        }

        /// <summary> Compares a current and previous version of an image and returns a diff image if they differ </summary>
        /// <param name="self">The current version of the image </param>
        /// <param name="imgFile"> The file where the previous version of the image is stored in </param>
        /// <param name="maxAllowedDiff"> A good value to start testing with would be e.g. 0.0005 </param>
        /// <returns> null if the images do not differ, a diff image otherwise </returns>
        public static FileEntry Compare(this MagickImage self, FileEntry imgFile, ErrorMetric errorMetric, double maxAllowedDiff) {
            using (MagickImage oldImg = new MagickImage()) {
                oldImg.LoadFromFileEntry(imgFile);

                double diffValue = self.CompareV2(oldImg, errorMetric, out MagickImage imgWithDifferences);
                var diffDetected = diffValue > maxAllowedDiff;
                // Log.d($"Visual difference of current scene VS image '{imgFile.Name}' is: {diffValue} vs {maxAllowedDiff} (max allowed diff)");
                FileEntry diffFile = imgFile.Parent.GetChild(imgFile.NameWithoutExtension + ".diff" + imgFile.ExtensionWithDot);
                imgWithDifferences.SaveToFileEntry(diffFile);
                imgWithDifferences.Dispose();
                return diffDetected ? diffFile : null;
            }
        }

        public static double CompareV2(this MagickImage self, MagickImage oldImg, ErrorMetric errorMetric, out MagickImage imgWithDifferences) {
            // Check if both images have the same size:
            bool isNewImageSameSizeAsOldImage = oldImg.Width == self.Width && oldImg.Height == self.Height;
            if (!isNewImageSameSizeAsOldImage) {
                var diffSizesWarn = $"Different sizes: OldImage=({oldImg.Width}X{oldImg.Height}) but NewImage=({self.Width}X{self.Height})";
                bool isNewImageAspectRatioSameAsOldImage = self.GetAspectRatio() == oldImg.GetAspectRatio();
                if (isNewImageAspectRatioSameAsOldImage) {
                    Log.w(diffSizesWarn);
                    // If aspect ratio matches, resize both images to same size:
                    var minWidth = Math.Min(oldImg.Width, self.Width);
                    var minHeight = Math.Min(oldImg.Height, self.Height);
                    oldImg.Resize(minWidth, minHeight);
                    self.Resize(minWidth, minHeight);
                } else { // Missmatch in image sizes cant be fixed, abort:
                    throw new ArgumentException("Can't compare! " + diffSizesWarn);
                }
            }
            AssertV2.AreEqual(oldImg.Width, self.Width, "RegressionImage.Width");
            AssertV2.AreEqual(oldImg.Height, self.Height, "RegressionImage.Height");
            //return oldImg.Compare(self, out imgWithDifferences);
            imgWithDifferences = new MagickImage();
            return oldImg.Compare(self, new CompareSettings() { Metric = errorMetric }, imgWithDifferences);
        }

        public static float GetAspectRatio(this MagickImage self) { return self.Width / (float)self.Height; }

    }

}

#endif