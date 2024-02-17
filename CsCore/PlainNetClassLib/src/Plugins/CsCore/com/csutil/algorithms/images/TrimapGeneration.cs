using System;

namespace com.csutil.algorithms.images {
    public static class TrimapGeneration {
        public static byte[] FromFloodFill(byte[] floodFilled, int width, int height, int bytesPerPixel, int dilationKernel) {
            var trimap = new byte[floodFilled.Length];
            var dilatedFill = Filter.Dilate(floodFilled, width, height, bytesPerPixel, dilationKernel);

            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    if (CanBeSemiTranparent(floodFilled, dilatedFill, width, x, y, bytesPerPixel)) {
                        SetColorAt(trimap, x, y, width, new byte[] {128, 128, 128, 128}, 4);
                    } else {
                        var color = GetColorAt(floodFilled, x, y, bytesPerPixel, width);
                        SetColorAt(trimap, x, y, width, color, bytesPerPixel);
                    }
                }
            }
            return trimap;
        }
        private static bool CanBeSemiTranparent(byte[] floodFilled, byte[] dilatedFill, int width, int x, int y, int bytesPerPixel) {
            var floodColor = GetColorAt(floodFilled, x, y, bytesPerPixel, width);
            var dilatedColor = GetColorAt(dilatedFill, x, y, bytesPerPixel, width);
            return dilatedColor[0] == 255 && floodColor[0] == 0;
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