using StbImageSharp;
using System.Drawing;

namespace com.csutil.src.Plugins.CsCore.com.csutil.algorithms
{
    public class AaaaImageCompareAaaa {

        public AaaaImageCompareAaaa() { }

        public ImageResult ImageCompare(ImageResult img1, ImageResult img2) {

            ImageResult resultImage = new ImageResult();
            resultImage.Data = img1.Data.DeepCopy();
            resultImage.Width = img1.Width;
            resultImage.Height = img1.Height; 

            // Check if images are the same size
            if (img1.Width != img2.Width && img1.Height != img2.Height) {
                return resultImage;
            }

            Color[] colors_img1 = LoadImageColors(img1);
            Color[] colors_img2 = LoadImageColors(img2);

            // Compare images
            for (int i = 0; i < colors_img1.Length; i++) {

                if (colors_img1[i] != colors_img2[i]) {

                    ParseColorIntoData(resultImage, Color.FromArgb(255, 255, 0, 0), i);

                }

            }

            return resultImage;

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

    }

}
