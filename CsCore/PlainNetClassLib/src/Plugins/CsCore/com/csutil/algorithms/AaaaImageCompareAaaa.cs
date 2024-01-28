using System;
using StbImageSharp.Utility;
using StbImageSharp.Decoding;
using com.csutil.io;
using StbImageSharp;
using System.Drawing;
using System.Linq;

namespace com.csutil.src.Plugins.CsCore.com.csutil.algorithms
{
    public class AaaaImageCompareAaaa {

        public AaaaImageCompareAaaa() {



        }

        public bool ImageCompare(ImageResult img1, ImageResult img2) {

            // Check if images are the same size
            if (img1.Width != img2.Width && img1.Height != img2.Height) {
                return false;
            }

            Color[] colors_img1 = LoadImageColors(img1);
            Color[] colors_img2 = LoadImageColors(img2);

            // Compare images
            for (int i = 0; i < img1.Width; i++) {

                if (colors_img1[i] != colors_img2[i]) {
                    return false;
                }

            }

            return true;

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

    }

}
