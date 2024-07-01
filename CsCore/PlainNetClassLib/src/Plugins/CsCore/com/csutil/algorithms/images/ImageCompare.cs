using ImageMagick;
using StbImageSharp;
using System;
using System.Drawing;
using System.Numerics;
using com.csutil.algorithms.images;
using System.ComponentModel;

namespace com.csutil.src.Plugins.CsCore.com.csutil.algorithms {

    /// <summary>
    /// Class to compare two images and get the Mean Squared Error and the highlighted differences.
    /// </summary>
    public class ImageCompare {

        public ImageCompare() { }

        /// <summary> Compares two images </summary>
        /// <param name="img1"> Base image </param>
        /// <param name="img2"> image that gets compared to the base image </param>
        /// <param name="redScale"> output intensity of the red highlights of the differences between pictures</param>
        /// <returns> picture of visualized differences and Mean Squared Error of the two pictures </returns>
        public CompareResult CompareImage(ImageResult img1, ImageResult img2, double redScale=5) {
            CompareResult result = new CompareResult();
            ImageResult resultImage = new ImageResult();
            resultImage.Data = new byte[img1.Data.Length];
            for (int i = 0; i < img1.Data.Length; i++) {
                resultImage.Data[i] = img1.Data[i];
            }
            resultImage.Width = img1.Width;
            resultImage.Height = img1.Height; 
            // Check if images are the same size
            if (img1.Width != img2.Width && img1.Height != img2.Height) {
                result.resultImage = resultImage;
                return result;
            }
            // Load pixel colors
            ImageResultV2 pixels_img1 = LoadImageColors(img1, img1.Width, img1.Height);
            ImageResultV2 pixels_img2 = LoadImageColors(img2, img2.Width, img2.Height);
            // Calculate distortion
            result.distortion = GetMeanSquaredDistortion(pixels_img1.pixels, pixels_img2.pixels, resultImage, redScale);
            result.resultImage = resultImage;
            return result;
        }

        /// <summary> Loads Image Colors of image's data byte array </summary>
        /// <param name="img"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns> ImageResultV2 wrapper class for Color data </returns>
        private ImageResultV2 LoadImageColors(ImageResult img, int width, int height) {
            ImageResultV2 imageResultV2 = new ImageResultV2();
            Mat<int> pixels = new Mat<int>(width, height, 4);
            int[] color = new int[4];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    color[0] = img.Data[(y * width + x) * 4];
                    color[1] = img.Data[(y * width + x) * 4 + 1];
                    color[2] = img.Data[(y * width + x) * 4 + 2];
                    color[3] = img.Data[(y * width + x) * 4 + 3];
                    pixels.SetPixel(x, y, color);
                }
            }
            imageResultV2.pixels = pixels;
            return imageResultV2;
        }

        private void ParseColorIntoData(ImageResult img, Color color, int index) {
            img.Data[index * 4] = color.R;
            img.Data[index * 4 + 1] = color.G;
            img.Data[index * 4 + 2] = color.B;
            img.Data[index * 4 + 3] = color.A;
        }

        /// <summary> </summary>
        /// <param name="pixels_img1"> Base picture pixels info </param>
        /// <param name="pixels_img2"></param>
        /// <param name="resultImage"> Red highlighted image container </param>
        /// <param name="redScale"> output intensity of the red highlights of the differences between pictures</param>
        /// <returns> Mean Squared Error of input pictures' data </returns>
        private double GetMeanSquaredDistortion(Mat<int> pixels_img1, Mat<int> pixels_img2, ImageResult resultImage, double redScale) {
            double squaredMeanSum = 0.0d;
            // Compare images
            for (int y = 0; y < pixels_img1.Height; y++) {
                for (int x = 0; x < pixels_img1.Width; x++) {
                    Vector3 diff = new Vector3(
                        pixels_img1.GetPixel(x, y)[0] - pixels_img2.GetPixel(x, y)[0],
                        pixels_img1.GetPixel(x, y)[1] - pixels_img2.GetPixel(x, y)[1],
                        pixels_img1.GetPixel(x, y)[2] - pixels_img2.GetPixel(x, y)[2]
                    );
                    squaredMeanSum += diff.LengthSquared();
                    if (pixels_img1.GetPixel(x, y) != pixels_img2.GetPixel(x, y)) {
                        // 255^2 * 3 = 195075
                        double ratio = diff.LengthSquared() / 195075.0d;
                        int red = (int)Math.Max(0, Math.Min(255, pixels_img1.GetPixel(x, y)[0] * redScale * ratio + pixels_img1.GetPixel(x, y)[0]));
                        ParseColorIntoData(resultImage, Color.FromArgb(255, red, pixels_img1.GetPixel(x, y)[1], pixels_img1.GetPixel(x, y)[2]), y * pixels_img1.Width + x);
                    }
                }
            }
            return squaredMeanSum / (pixels_img1.Width * pixels_img1.Height);
        }

        public class CompareResult {
            public ImageResult resultImage;
            public double distortion;
        }

        public class ImageResultV2 {
            public Mat<int> pixels;
        }

    }

}
