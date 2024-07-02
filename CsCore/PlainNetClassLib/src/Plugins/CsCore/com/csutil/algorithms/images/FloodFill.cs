using System;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    public static class FloodFill {

        private static readonly byte[] white = new byte[] { 255, 255, 255, 255 };

        public static byte[] RunFloodFillAlgorithm(this ImageResult self, int colorThreshold = 240) {
            var image = self.Data.DeepCopy();
            var width = self.Width;
            var height = self.Height;

            var result = new byte[image.Length];
            for (var a = 3; a < image.Length; a += 4) {
                result[a] = 255;
            }

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (SeemsValue(image, x, y, width, colorThreshold)) {
                        SetColorAt(result, x, y, width, white, 4);
                    }
                }
            }
            return result;
        }

        private static bool SeemsValue(byte[] image, int x, int y, int width, int colorThreshold) {
            var color = GetColorAt(image, x, y, 4, width);
            return color[0] > colorThreshold && color[1] > colorThreshold && color[2] > colorThreshold;
        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            var startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            var startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }

    }

}