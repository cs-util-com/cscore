using System;
using System.Collections.Generic;
using System.Text;
namespace com.csutil.algorithms.images {
    public static class ImageBlur {

        // Helper method to set the color at a specific location in the image array
        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            var colorWithOriginalAlpha = new byte[] { color[0], color[1], color[2], imageData[startIdx + 3] };
            Array.Copy(colorWithOriginalAlpha, 0, imageData, startIdx, color.Length);
        }

        // Helper method to get color at a given position
        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }

        //Potentially optimizable by skipping pixels already in for loop instead of checking throgh if statement 

        //Todo fix for alpha value to always be the same alpha as the pixel originally had
        public static byte[] RunBoxBlur(byte[] image, int width, int height, int halfKernel, int bytePerPixel) {
            var result = new byte[image.Length];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var sum = new int[bytePerPixel];
                    var count = 0;
                    for (var ky = -halfKernel; ky <= halfKernel; ky++) {
                        for (var kx = -halfKernel; kx <= halfKernel; kx++) {
                            var offsetX = x + kx;
                            var offsetY = y + ky;

                            if (offsetX < 0 || offsetX >= width || offsetY < 0 || offsetY >= height) continue;

                            var color = GetColorAt(image, offsetX, offsetY, bytePerPixel, width);
                            for (var channel = 0; channel < bytePerPixel; channel++) {
                                sum[channel] += color[channel];
                            }
                            count++;
                        }
                    }
                    var byteSum = new byte[bytePerPixel];
                    for (var channel = 0; channel < bytePerPixel; channel++) {
                        sum[channel] /= count;
                        byteSum[channel] = (byte)sum[channel];
                    }


                    SetColorAt(result, x, y, width, byteSum, bytePerPixel);
                }
            }

            return result;
        }
        
        public static double[] RunBoxBlurDouble(double[] image, int width, int height, int halfKernel, int bytePerPixel) {
            var result = new double[image.Length];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var sum = new double[bytePerPixel];
                    var count = 0;
                    for (var ky = -halfKernel; ky <= halfKernel; ky++) {
                        for (var kx = -halfKernel; kx <= halfKernel; kx++) {
                            var offsetX = x + kx;
                            var offsetY = y + ky;

                            if (offsetX < 0 || offsetX >= width || offsetY < 0 || offsetY >= height) continue;

                            var color = GetColorAtDouble(image, offsetX, offsetY, bytePerPixel, width);
                            for (var channel = 0; channel < bytePerPixel; channel++) {
                                sum[channel] += color[channel];
                            }
                            count++;
                        }
                    }
                    for (var channel = 0; channel < bytePerPixel; channel++) {
                        sum[channel] /= count;
                    }


                    SetColorAtDouble(result, x, y, width, sum, bytePerPixel);
                }
            }

            return result;
        }
        
        private static void SetColorAtDouble(double[] imageData, int x, int y, int width, double[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            var colorWithOriginalAlpha = new [] { color[0], color[1], color[2], imageData[startIdx + 3] };
            Array.Copy(colorWithOriginalAlpha, 0, imageData, startIdx, color.Length);
        }

        // Helper method to get color at a given position
        private static double[] GetColorAtDouble(double[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new double[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }
    }
}