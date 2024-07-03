using System;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    public static class FloodFill {

        private static readonly byte[] white = new byte[] { 255, 255, 255, 255 };

        public static byte[] RunColorCheckAlgorithm(this ImageResult self, int colorThreshold = 240) {
            var image = self.Data.DeepCopy();
            var width = self.Width;
            var height = self.Height;
            var bytesPerPixel = (int)self.ColorComponents;

            var result = new byte[image.Length];
            for (var a = 3; a < image.Length; a += 4) {
                result[a] = 255;
            }

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    var startIdx = (y * width + x) * bytesPerPixel;
                    var r = image[startIdx];
                    var g = image[startIdx + 1];
                    var b = image[startIdx + 2];
                    var a = image[startIdx + 3];
                    var isBackground = r > colorThreshold && g > colorThreshold && b > colorThreshold;
                    if (!isBackground) {
                        SetColorAt(result, x, y, width, white, 4);
                    }
                }
            }
            return result;
        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            var startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

    }

}