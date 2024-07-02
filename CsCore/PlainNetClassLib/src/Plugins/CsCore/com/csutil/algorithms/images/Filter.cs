using System;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    public static class Filter {

        public static byte[] RunBoxFilter(this ImageResult image, int boxFilterRadius) {
            return BoxFilter(image.Data, image.Width, image.Height, boxFilterRadius, (int)image.ColorComponents);
        }

        private static byte[] BoxFilter(byte[] img, int width, int height, int radius, int channels) {
            var kernalSize = 2 * radius;

            if (kernalSize % 2 == 0) kernalSize++;
            var hBlur = new byte[img.Length];
            var avg = (float)1 / kernalSize;
            Array.Copy(img, hBlur, img.Length);
            for (int j = 0; j < height; j++) {
                var hSum = new float[] { 0f, 0f, 0f, 0f, };
                var iAvg = new float[] { 0f, 0f, 0f, 0f, };
                for (int x = 0; x < kernalSize; x++) {
                    var tmpColor = GetColorAt(img, x, j, channels, width);
                    hSum[0] += (float)tmpColor[0];
                    hSum[1] += (float)tmpColor[1];
                    hSum[2] += (float)tmpColor[2];
                    hSum[3] += (float)tmpColor[3];
                }
                iAvg[0] = hSum[0] * avg;
                iAvg[1] = hSum[1] * avg;
                iAvg[2] = hSum[2] * avg;
                iAvg[3] = hSum[3] * avg;
                for (int i = 0; i < width; i++) {
                    if (i - kernalSize / 2 >= 0 && i + 1 + kernalSize / 2 < width) {
                        var tmp_pColor = GetColorAt(img, i - kernalSize / 2, j, channels, width);
                        hSum[0] -= (float)tmp_pColor[0];
                        hSum[1] -= (float)tmp_pColor[1];
                        hSum[2] -= (float)tmp_pColor[2];
                        hSum[3] -= (float)tmp_pColor[3];
                        var tmp_nColor = GetColorAt(img, i + 1 + kernalSize / 2, j, channels, width);
                        hSum[0] += (float)tmp_nColor[0];
                        hSum[1] += (float)tmp_nColor[1];
                        hSum[2] += (float)tmp_nColor[2];
                        hSum[3] += (float)tmp_nColor[3];
                        //
                        iAvg[0] = hSum[0] * avg;
                        iAvg[1] = hSum[1] * avg;
                        iAvg[2] = hSum[2] * avg;
                        iAvg[3] = hSum[3] * avg;
                    }
                    var bAvg = new byte[iAvg.Length];
                    bAvg[0] = (byte)iAvg[0];
                    bAvg[1] = (byte)iAvg[1];
                    bAvg[2] = (byte)iAvg[2];
                    bAvg[3] = (byte)iAvg[3];
                    SetColorAt(hBlur, i, j, width, bAvg, channels);
                }

            }
            var total = new byte[hBlur.Length];
            Array.Copy(hBlur, total, hBlur.Length);
            for (int i = 0; i < width; i++) {
                var tSum = new float[] { 0f, 0f, 0f, 0f };
                var iAvg = new float[] { 0f, 0f, 0f, 0f };
                for (int y = 0; y < kernalSize; y++) {
                    var tmpColor = GetColorAt(hBlur, i, y, channels, width);
                    tSum[0] += (float)tmpColor[0];
                    tSum[1] += (float)tmpColor[1];
                    tSum[2] += (float)tmpColor[2];
                    tSum[3] += (float)tmpColor[3];
                }
                iAvg[0] = tSum[0] * avg;
                iAvg[1] = tSum[1] * avg;
                iAvg[2] = tSum[2] * avg;
                iAvg[3] = tSum[3] * avg;

                for (int j = 0; j < height; j++) {
                    if (j - kernalSize / 2 >= 0 && j + 1 + kernalSize / 2 < height) {
                        var tmp_pColor = GetColorAt(hBlur, i, j - kernalSize / 2, channels, width);
                        tSum[0] -= (float)tmp_pColor[0];
                        tSum[1] -= (float)tmp_pColor[1];
                        tSum[2] -= (float)tmp_pColor[2];
                        tSum[3] -= (float)tmp_pColor[3];
                        var tmp_nColor = GetColorAt(hBlur, i, j + 1 + kernalSize / 2, channels, width);
                        tSum[0] += (float)tmp_nColor[0];
                        tSum[1] += (float)tmp_nColor[1];
                        tSum[2] += (float)tmp_nColor[2];
                        tSum[3] += (float)tmp_nColor[3];

                        iAvg[0] = tSum[0] * avg;
                        iAvg[1] = tSum[1] * avg;
                        iAvg[2] = tSum[2] * avg;
                        iAvg[3] = tSum[3] * avg;

                    }
                    var bAvg = new byte[iAvg.Length];
                    bAvg[0] = (byte)iAvg[0];
                    bAvg[1] = (byte)iAvg[1];
                    bAvg[2] = (byte)iAvg[2];
                    bAvg[3] = (byte)iAvg[3];
                    SetColorAt(total, i, j, width, bAvg, channels);
                }
            }
            return total;

        }

        private static void SetColorAt(byte[] imageData, int x, int y, int width, byte[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        public static double[] BoxFilterSingleChannel(double[] img, int width, int height, int radius, int channels) {
            var kernalSize = 2 * radius;

            if (kernalSize % 2 == 0) kernalSize++;
            var hBlur = new double[img.Length];
            var avg = (double)1 / kernalSize;
            Array.Copy(img, hBlur, img.Length);
            for (int j = 0; j < height; j++) {
                var hSum = new double[] { 0f };
                var iAvg = new double[] { 0f };
                for (int x = 0; x < kernalSize; x++) {
                    var tmpColor = GetColorAt(img, x, j, channels, width);
                    hSum[0] += (double)tmpColor[0];
                }
                iAvg[0] = hSum[0] * avg;
                for (int i = 0; i < width; i++) {
                    if (i - kernalSize / 2 >= 0 && i + 1 + kernalSize / 2 < width) {
                        var tmp_pColor = GetColorAt(img, i - kernalSize / 2, j, channels, width);
                        hSum[0] -= (double)tmp_pColor[0];
                        var tmp_nColor = GetColorAt(img, i + 1 + kernalSize / 2, j, channels, width);
                        hSum[0] += (double)tmp_nColor[0];
                        //
                        iAvg[0] = hSum[0] * avg;
                    }
                    var bAvg = new double[iAvg.Length];
                    bAvg[0] = (double)iAvg[0];
                    SetColorAt(hBlur, i, j, width, bAvg, channels);

                }

            }
            var total = new double[hBlur.Length];
            Array.Copy(hBlur, total, hBlur.Length);
            for (int i = 0; i < width; i++) {
                var tSum = new double[] { 0f };
                var iAvg = new double[] { 0f };
                for (int y = 0; y < kernalSize; y++) {
                    var tmpColor = GetColorAt(hBlur, i, y, channels, width);
                    tSum[0] += (double)tmpColor[0];
                }
                iAvg[0] = tSum[0] * avg;

                for (int j = 0; j < height; j++) {
                    if (j - kernalSize / 2 >= 0 && j + 1 + kernalSize / 2 < height) {
                        var tmp_pColor = GetColorAt(hBlur, i, j - kernalSize / 2, channels, width);
                        tSum[0] -= (double)tmp_pColor[0];
                        var tmp_nColor = GetColorAt(hBlur, i, j + 1 + kernalSize / 2, channels, width);
                        tSum[0] += (double)tmp_nColor[0];

                        iAvg[0] = tSum[0] * avg;

                    }
                    var bAvg = new double[iAvg.Length];
                    bAvg[0] = (double)iAvg[0];
                    SetColorAt(total, i, j, width, bAvg, channels);
                }
            }
            return total;
        }

        // Helper method to get color at a given position
        private static byte[] GetColorAt(byte[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            return new byte[] { img[startIdx], img[startIdx + 1], img[startIdx + 2], img[startIdx + 3] };
        }

        private static void SetColorAt(double[] imageData, int x, int y, int width, double[] color, int bytesPerPixel) {
            int startIdx = (y * width + x) * bytesPerPixel;
            Array.Copy(color, 0, imageData, startIdx, color.Length);
        }

        // Helper method to get color at a given position
        private static double[] GetColorAt(double[] img, int x, int y, int bytesPerPixel, int width) {
            int startIdx = (y * width + x) * bytesPerPixel;
            var color = new double[bytesPerPixel];
            for (int i = 0; i < bytesPerPixel; i++) {
                color[i] = img[startIdx + i];
            }
            return color;
        }

        public static byte[] Erode(byte[] image, int width, int height, int bytePerPixel, int kernelSize) {
            var intermediateResult = Erosion1D(image, width, height, bytePerPixel, kernelSize, true);
            return Erosion1D(intermediateResult, width, height, bytePerPixel, kernelSize, false);
        }

        private static byte[] Erosion1D(byte[] imageData, int width, int height, int bytePerPixel, int kernelSize, bool horizontal) {
            var erodedImage = imageData.DeepCopy();
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    var erodePixel = false;
                    for (int k = -kernelSize; k <= kernelSize; k++) {
                        var pixelX = horizontal ? x + k : x;
                        var pixelY = horizontal ? y : y + k;
                        // continue if out of bounds
                        if (pixelX < 0 || pixelX >= width || pixelY < 0 || pixelY >= height) continue;
                        var pixelIndex = (pixelY * width + pixelX) * bytePerPixel;

                        var r = imageData[pixelIndex];
                        var g = imageData[pixelIndex + 1];
                        var b = imageData[pixelIndex + 2];

                        // If any channel is non-zero, the pixel is not part of the foreground
                        if (r != 0 && g != 0 && b != 0) continue;
                        erodePixel = true;
                        break;
                    }

                    // Set the pixel value in the eroded image
                    var currentIndex = (y * width + x) * bytePerPixel;
                    if (!erodePixel) continue;
                    // If all channels are 0, erode the pixel, but keep org alpha value
                    erodedImage[currentIndex] = 0;
                    erodedImage[currentIndex + 1] = 0;
                    erodedImage[currentIndex + 2] = 0;
                }
            }
            return erodedImage;
        }

        public static byte[] Dilate(byte[] imageData, int width, int height, int bytePerPixel, int kernelSize) {
            var intermediateResult = Dilation1D(imageData, width, height, bytePerPixel, kernelSize, true);
            return Dilation1D(intermediateResult, width, height, bytePerPixel, kernelSize, false);
        }

        private static byte[] Dilation1D(byte[] imageData, int width, int height, int bytePerPixel, int kernelSize, bool horizontal) {
            byte[] dilatedImage = new byte[imageData.Length];

            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    byte maxR = 0, maxG = 0, maxB = 0, maxA = 0;
                    // Apply 1D kernel
                    for (int k = -kernelSize; k <= kernelSize; k++) {
                        var pixelX = horizontal ? x + k : x;
                        var pixelY = horizontal ? y : y + k;
                        if (pixelX < 0 || pixelX >= width || pixelY < 0 || pixelY >= height) continue;
                        var pixelIndex = (pixelY * width + pixelX) * bytePerPixel;

                        var r = imageData[pixelIndex];
                        var g = imageData[pixelIndex + 1];
                        var b = imageData[pixelIndex + 2];
                        var a = imageData[pixelIndex + 3];

                        maxR = Math.Max(maxR, r);
                        maxG = Math.Max(maxG, g);
                        maxB = Math.Max(maxB, b);
                        maxA = Math.Max(maxA, a);
                    }

                    var currentIndex = (y * width + x) * bytePerPixel;
                    dilatedImage[currentIndex] = maxR;
                    dilatedImage[currentIndex + 1] = maxG;
                    dilatedImage[currentIndex + 2] = maxB;
                    dilatedImage[currentIndex + 3] = maxA;
                }
            }
            return dilatedImage;
        }

        public static byte[] DilateNaive(byte[] input, int width, int height, int bytesPerPixel, int kernelSize) {
            var dilatedImage = new byte[input.Length];

            // Loop through each pixel in the image
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    // Find the maximum pixel value in the neighborhood defined by the kernel
                    byte maxR = 0, maxG = 0, maxB = 0, maxA = 0;

                    for (int ky = -kernelSize; ky <= kernelSize; ky++) {
                        for (int kx = -kernelSize; kx <= kernelSize; kx++) {
                            var pixelX = x + kx;
                            var pixelY = y + ky;

                            // Ensure the pixel is within bounds
                            if (pixelX >= 0 && pixelX < width && pixelY >= 0 && pixelY < height) {
                                var pixelIndex = (pixelY * width + pixelX) * bytesPerPixel;

                                // Extract RGBA values
                                var r = input[pixelIndex];
                                var g = input[pixelIndex + 1];
                                var b = input[pixelIndex + 2];
                                var a = input[pixelIndex + 3];

                                // Update maximum values
                                maxR = Math.Max(maxR, r);
                                maxG = Math.Max(maxG, g);
                                maxB = Math.Max(maxB, b);
                                maxA = Math.Max(maxA, a);
                            }
                        }
                    }

                    // Set the pixel value in the dilated image
                    var currentIndex = (y * width + x) * bytesPerPixel;
                    dilatedImage[currentIndex] = maxR;
                    dilatedImage[currentIndex + 1] = maxG;
                    dilatedImage[currentIndex + 2] = maxB;
                    dilatedImage[currentIndex + 3] = maxA;
                }
            }
            return dilatedImage;
        }

    }

}