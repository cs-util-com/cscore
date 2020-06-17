using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Zio;

namespace com.csutil {

    public static class MagickImageExtensions {

        public static void SaveToFileEntry(this MagickImage self, FileEntry f) {
            using (Stream outStream = f.OpenOrCreateForWrite()) { self.Write(outStream); }
        }

        public static void LoadFromFileEntry(this MagickImage self, FileEntry f) {
            using (Stream inputStream = f.OpenForRead()) { self.Read(inputStream); }
        }

        public static double Compare(this MagickImage self, MagickImage newImg, out MagickImage diffImg, ErrorMetric errorMetric = ErrorMetric.MeanSquared) {
            diffImg = new MagickImage();
            return self.Compare(newImg, new CompareSettings() { Metric = errorMetric }, diffImg);
        }

        public static FileEntry Compare(this MagickImage self, FileEntry imgFile, double MAX_ALLOWED_DIFF = 0.0005) {
            using (MagickImage oldImg = new MagickImage()) {
                oldImg.LoadFromFileEntry(imgFile);

                AssertV2.AreEqual(oldImg.Width, self.Width, "RegressionImage.Width");
                AssertV2.AreEqual(oldImg.Height, self.Height, "RegressionImage.Height");
                // Check if both images have the same size:
                if (oldImg.Width != self.Width || oldImg.Height != self.Height) {
                    if (self.GetAspectRatio() == oldImg.GetAspectRatio()) {
                        // If aspect ratio matches, resize both images to same size:
                        var minWidth = Math.Min(oldImg.Width, self.Width);
                        var minHeight = Math.Min(oldImg.Height, self.Height);
                        oldImg.Resize(minWidth, minHeight);
                        self.Resize(minWidth, minHeight);
                    }
                }

                FileEntry diffFile = imgFile.Parent.GetChild(imgFile.NameWithoutExtension + "_diff" + imgFile.ExtensionWithDot);
                var diffValue = oldImg.Compare(self, out MagickImage imgWithDifferences);
                Log.d($"Visual difference of current scene VS image '{imgFile.Name}' is: {diffValue}");
                var diffDetected = diffValue > MAX_ALLOWED_DIFF;
                imgWithDifferences.SaveToFileEntry(diffFile);
                imgWithDifferences.Dispose();
                return diffDetected ? diffFile : null;
            }
        }

        private static float GetAspectRatio(this MagickImage self) { return self.Width / (float)self.Height; }
    }

}