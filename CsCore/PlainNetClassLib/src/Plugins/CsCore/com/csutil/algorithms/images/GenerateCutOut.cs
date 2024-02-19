using System;
using ImageMagick;
using StbImageSharp;

namespace com.csutil.algorithms.images {
    public static class GenerateCutOut {
        
        
        /// <summary>
        /// Generates a cutout from the original data in imageRes. The trimap is created by flooding the original from the border according to the
        /// floodValue and then dilating and eroding this region to generate the area where semi transparency might exist.
        /// </summary>
        /// <param name="imageRes"></param> ImageResult data which has the image data and its properties
        /// <param name="floodValue"></param> Value used as threshold to where the image shall set pixels to 0 for the flooded region
        /// <param name="kernelSize"></param> Size of the box used for dilation, erosion in the trimap generation
        /// <param name="eps"></param> Epsilon parameter of the guided filter
        /// <param name="cutoffValue"></param> All alpha values below this get set to 0, above are kept as they were in the alpha map
        /// <returns></returns>
        public static byte[] Generate(ImageResult imageRes, int floodValue, int kernelSize, double eps, int cutoffValue) {
            var image = imageRes.Data.DeepCopy();
            var width = imageRes.Width;
            var height = imageRes.Height;
            var bytesPerPixel = (int)imageRes.ColorComponents;
            
            
            var floodFilled = FloodFill.FloodFillAlgorithm(imageRes, floodValue);
            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, (int)imageRes.ColorComponents, 10);
            
            var imageMatting = new GlobalMatting(image, width, height, bytesPerPixel);
            imageMatting.ExpansionOfKnownRegions(ref trimap, niter: 9);
            imageMatting.RunGlobalMatting(trimap, out var foreground, out var alphaData, out var conf);
            // filter the result with fast guided filter
            var alpha = imageMatting.RunGuidedFilter(alphaData, kernelSize, eps);
            
            
            for (int x = 0; x < width; ++x) {
                for (int y = 0; y < height; ++y) {
                    if (GetColorAt(trimap, x, y, bytesPerPixel, width)[0] == 0) {
                        SetColorAt(alpha, x, y, width, new byte[]{0, 0, 0, 0}, bytesPerPixel);
                    } else if (GetColorAt(trimap, x, y, bytesPerPixel, width)[0] == 255) {
                        SetColorAt(alpha, x, y, width, new byte[]{255, 255, 255, 255}, bytesPerPixel);
                    }
                }
            }
            // Safe cut out according to alpha region that is >= cutoffValue
            var cutout = image.DeepCopy();
            
            for (var x = 0; x < width; ++x) {
                for (var y = 0; y < height; ++y) {
                    var value = (int)GetColorAt(alpha, x, y, bytesPerPixel, width)[3];
                    var idx = (y * width + x) * bytesPerPixel;
                    cutout[idx + 3] = value >= cutoffValue ? (byte)value : (byte)0;
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