using System;
using ImageMagick;

namespace com.csutil.algorithms.images {
    public static class GenerateCutOut {
        public static byte[] Generate(byte[] image, byte[] trimap, int width, int height, int bytesPerPixel) {
            var imageMatting = new GlobalMatting(image.DeepCopy(), width, height, bytesPerPixel);
            imageMatting.ExpansionOfKnownRegions(ref trimap, niter: 9);
            imageMatting.RunGlobalMatting(trimap, out var foreground, out var alphaData, out var conf);
            // filter the result with fast guided filter
            var alpha = imageMatting.RunGuidedFilter(alphaData, r: 10, eps: 1e-5);
            
            
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    if (GetColorAt(trimap, x, y, bytesPerPixel, width)[0] == 0) {
                        SetColorAt(alpha, x, y, width, new byte[]{0, 0, 0, 0}, bytesPerPixel);
                    } else if (GetColorAt(trimap, x, y, bytesPerPixel, width)[0] == 255) {
                        SetColorAt(alpha, x, y, width, new byte[]{255, 255, 255, 255}, bytesPerPixel);
                    }
                }
            }
            // Safe cut out according to alpha region that is >= 128
            var cutoffValue = 129;
            var cutout = image.DeepCopy();
            
            for (var x = 0; x < width; ++x) {
                for (var y = 0; y < height; ++y) {
                    var value = (int)GetColorAt(alpha, x, y, bytesPerPixel, width)[3];
                    var idx = (y * width + x) * bytesPerPixel;
                    cutout[idx + 3] = value >= cutoffValue ? (byte)value : (byte)0;
                    // cutout[idx + 3] = (byte)value;
                }
            }
            return cutout;
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