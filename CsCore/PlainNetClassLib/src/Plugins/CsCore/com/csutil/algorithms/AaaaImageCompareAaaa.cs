using StbImageSharp;
using System;
using System.Drawing;
using System.Numerics;

namespace com.csutil.src.Plugins.CsCore.com.csutil.algorithms {

    public class AaaaImageCompareAaaa {

        public AaaaImageCompareAaaa() { }

        public CompareResult ImageCompare(ImageResult img1, ImageResult img2) {

            CompareResult result = new CompareResult();

            ImageResult resultImage = new ImageResult();
            resultImage.Data = img1.Data.DeepCopy();
            resultImage.Width = img1.Width;
            resultImage.Height = img1.Height; 

            // Check if images are the same size
            if (img1.Width != img2.Width && img1.Height != img2.Height) {
                result.resultImage = resultImage;
                return result;
            }

            Color[] colors_img1 = LoadImageColors(img1);
            Color[] colors_img2 = LoadImageColors(img2);
            double squaredMeanSum = 0.0d;

            // Compare images
            for (int i = 0; i < colors_img1.Length; i++) {

                // Get pixel distortion
                Vector3 diff = new Vector3(
                    colors_img1[i].R - colors_img2[i].R,
                    colors_img1[i].G - colors_img2[i].G,
                    colors_img1[i].B - colors_img2[i].B
                );

                squaredMeanSum += diff.LengthSquared();

                if (colors_img1[i] != colors_img2[i]) {

                    double ratio = diff.LengthSquared() / 195075.0d;

                    int red = (int)Math.Max(0, Math.Min(255, colors_img1[i].R * 15 * ratio + colors_img1[i].R));
                    ParseColorIntoData(resultImage, Color.FromArgb(255, red, colors_img1[i].G, colors_img1[i].B), i);

                }

            }

            double squaredMean = squaredMeanSum / (img1.Width * img1.Height);

            result.resultImage = resultImage;
            result.distortion = squaredMean;

            return result;

        }

        private Color[] LoadImageColors(ImageResult img) {

            Color[] colors = new Color[img.Data.Length / 4];
            for (int i = 0; i < colors.Length; i++) {

                colors[i] = Color.FromArgb(
                        img.Data[i * 4 + 3],
                        img.Data[i * 4],
                        img.Data[i * 4 + 1],
                        img.Data[i * 4 + 2]
                    );

            }

            return colors;

        }

        private void ParseColorIntoData(ImageResult img, Color color, int index) {

            img.Data[index * 4] = color.R;
            img.Data[index * 4 + 1] = color.G;
            img.Data[index * 4 + 2] = color.B;
            img.Data[index * 4 + 3] = color.A;

        }

        public class CompareResult {

            public ImageResult resultImage;
            public double distortion;

        }

    }

}
