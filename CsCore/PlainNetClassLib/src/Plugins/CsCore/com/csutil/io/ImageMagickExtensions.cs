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

        public static FileEntry Compare(this MagickImage self, FileEntry imgFile, double MAX_ALLOWED_DIFF = 0.001) {
            using (MagickImage oldImg = new MagickImage()) {
                oldImg.LoadFromFileEntry(imgFile);
                FileEntry diffFile = imgFile.Parent.GetChild(imgFile.NameWithoutExtension + "_diff" + imgFile.ExtensionWithDot);
                var diffDetected = oldImg.Compare(self, out MagickImage imgWithDifferences) > MAX_ALLOWED_DIFF;
                imgWithDifferences.SaveToFileEntry(diffFile);
                imgWithDifferences.Dispose();
                return diffDetected ? diffFile : null;
            }
        }

    }

}