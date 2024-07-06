using System;

namespace com.csutil.algorithms.images {

    public static class TrimapGeneration {

        public static byte[] FromFloodFill(byte[] floodFilled, int width, int height, int bytesPerPixel, int kernel) {
            var dilatedFill = Filter.Dilate(floodFilled, width, height, bytesPerPixel, kernel);
            var erodedFill = Filter.Erode(floodFilled, width, height, bytesPerPixel, kernel);

            var trimap = new byte[floodFilled.Length];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (CanBeSemiTransparent(erodedFill, dilatedFill, width, x, y, bytesPerPixel)) {
                        SetColorAt(trimap, x, y, width, new byte[] { 128, 128, 128, 255 }, 4);
                    } else {
                        var color = GetColorAt(floodFilled, x, y, bytesPerPixel, width);
                        SetColorAt(trimap, x, y, width, color, bytesPerPixel);
                    }
                }
            }
            return trimap;
        }

        public static byte[] FromClosedFloodFill(byte[] floodFilled, int width, int height, int bytesPerPixel, int kernel) {
            // The floodfilled image alone can have a very noise border, so instead a closed (dilated then eroded) version could be used:
            var closed = Filter.Erode(Filter.Dilate(floodFilled, width, height, bytesPerPixel, kernel), width, height, bytesPerPixel, kernel);
            return FromFloodFill(closed, width, height, bytesPerPixel, kernel);
        }

        private static bool CanBeSemiTransparent(byte[] erodedFill, byte[] dilatedFill, int width, int x, int y, int bytesPerPixel) {
            var erodedColor = GetColorAt(erodedFill, x, y, bytesPerPixel, width);
            var dilatedColor = GetColorAt(dilatedFill, x, y, bytesPerPixel, width);
            return dilatedColor[0] == 255 && erodedColor[0] == 0;
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