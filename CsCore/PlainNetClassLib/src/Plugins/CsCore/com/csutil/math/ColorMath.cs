using System;

namespace com.csutil.math {

    public static class ColorMath {

        /// <summary> 
        /// Calculates the brightness using the relative luminanace:
        /// The relative brightness of any point in a colorspace, 
        /// normalized to 0 for darkest black and 1 for lightest white
        /// see https://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef 
        /// and https://stackoverflow.com/a/9733420/165106
        /// </summary>
        public static double CalcBrightness(double r, double g, double b) {
            if (r <= 0.03928) { r = r / 12.92; } else { r = Math.Pow((r + 0.055) / 1.055, 2.4); }
            if (g <= 0.03928) { g = g / 12.92; } else { g = Math.Pow((g + 0.055) / 1.055, 2.4); }
            if (b <= 0.03928) { b = b / 12.92; } else { b = Math.Pow((b + 0.055) / 1.055, 2.4); }
            return 0.2126 * r + 0.7152 * g + 0.0722 * b;
        }

        /// <summary>
        /// see https://ux.stackexchange.com/a/82068 
        /// and https://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef 
        /// </summary>
        /// <param name="brightness1"> use CalcBrightness(r,g,b) </param>
        /// <param name="brightness2"> use CalcBrightness(r,g,b) </param>
        /// <returns> a ratio > 4.5 is considered a "good" contrast </returns>
        public static double CalcContrastRatio(double brightness1, double brightness2) {
            if (brightness1 > brightness2) { return (brightness1 + 0.05) / (brightness2 + 0.05); }
            return (brightness2 + 0.05) / (brightness1 + 0.05);
        }

        /// <summary> Can be used to invert the color to its complimentary version </summary>
        public static void InvertHue(float[] hsv) { hsv[0] = (hsv[0] + 0.5f) % 1f; }

        /// <summary> calculates HSV (hue, saturation, value) for a given RGB (RGB values must be 0-1) </summary>
        /// <param name="r"> A value from 0 to 1 </param>
        /// <param name="g"> A value from 0 to 1 </param>
        /// <param name="b"> A value from 0 to 1 </param>
        /// <returns>HSV (hue, saturation, value)</returns>
        public static float[] RgbToHsv(float r, float g, float b) {
            var min = Min(r, g, b);
            var max = Max(r, g, b);
            var delta = max - min;
            var h = 0f;
            var s = 0f;
            var v = max;
            if (delta.Equals(0f)) { return new[] { h, s, v }; }
            s = delta / max;
            var dR = ((max - r) / 6f + delta / 2f) / delta;
            var dG = ((max - g) / 6f + delta / 2f) / delta;
            var dB = ((max - b) / 6f + delta / 2f) / delta;
            if (r.Equals(max)) {
                h = dB - dG;
            } else if (g.Equals(max)) {
                h = 1f / 3f + dR - dB;
            } else if (b.Equals(max)) {
                h = 2f / 3f + dG - dR;
            }
            if (h < 0f) { h += 1; } else if (h > 1f) { h -= 1; }
            return new[] { h, s, v };
        }

        private static float Min(float a, float b, float c) { return Math.Min(a, Math.Min(b, c)); }
        private static float Max(float a, float b, float c) { return Math.Max(a, Math.Max(b, c)); }

        public static float[] HsvToRgb(float hue, float saturation, float value) {
            hue = hue.Equals(1f) ? 0f : hue * 6f;
            var i = (int)hue;
            var r = value;
            var g = value;
            var b = value;
            switch (i) {
                case 0:
                    g = value * (1f - saturation * (1f - (hue - i)));
                    b = value * (1f - saturation);
                    break;
                case 1:
                    r = value * (1f - saturation * (hue - i));
                    b = value * (1f - saturation);
                    break;
                case 2:
                    r = value * (1f - saturation);
                    b = value * (1f - saturation * (1f - (hue - i)));
                    break;
                case 3:
                    r = value * (1f - saturation);
                    g = value * (1f - saturation * (hue - i));
                    break;
                case 4:
                    r = value * (1f - saturation * (1f - (hue - i)));
                    g = value * (1f - saturation);
                    break;
                case 5:
                    g = value * (1f - saturation);
                    b = value * (1f - saturation * (hue - i));
                    break;
            }
            return new float[3] { Round(r), Round(g), Round(b) };
        }
        
        private static float Round(float f) { return (float)Math.Round(f, 6); }

        public static float[] NextRandomRgbColor(this Random self) {
            float[] color = new float[3];
            // The division by 3 and the addition of 0.5 serve to make the distribution more skewed towards the middle of the range
            color[0] = MathF.Max(0, MathF.Min(1, (float)self.NextGaussian() / 3f + 0.5f)); // red
            color[1] = MathF.Max(0, MathF.Min(1, (float)self.NextGaussian() / 3f + 0.5f)); // green
            color[2] = MathF.Max(0, MathF.Min(1, (float)self.NextGaussian() / 3f + 0.5f)); // blue
            return color;
        }

        private static float[] white = new float[] { 1, 1, 1 };
        private static float[] red = new float[] { 1, 0, 0 };
        private static float[] yellow = new float[] { 1, 1, 0 };
        private static float[] blue = new float[] { 0.163f, 0.373f, 0.6f };
        private static float[] violet = new float[] { 0.5f, 0, 0.5f };
        private static float[] green = new float[] { 0, 0.66f, 0.2f };
        private static float[] orange = new float[] { 1, 0.5f, 0 };
        private static float[] black = new float[] { 0.2f, 0.094f, 0 };
        /// <summary>
        /// Returns a color with a maximized euclidean distance to the input color
        /// See also paper "Paint Inspired Color Mixing and Compositing for Visualization"
        /// </summary>
        public static float[] NextColorAfter(float red, float yellow, float blue) {
            float[] result = new float[3];
            for (int i = 0; i <= 2; i++) {
                result[i] = white[i] * (1 - red) * (1 - blue) * (1 - yellow) + ColorMath.red[i] * red * (1 - blue) * (1 - yellow) +
                    ColorMath.blue[i] * (1 - red) * blue * (1 - yellow) + violet[i] * red * blue * (1 - yellow) +
                    ColorMath.yellow[i] * (1 - red) * (1 - blue) * yellow + orange[i] * red * (1 - blue) * yellow +
                    green[i] * (1 - red) * blue * yellow + black[i] * red * blue * yellow;
            }
            return result;
        }

        /// <summary>
        /// Returns a color with a maximized euclidean distance to the input color
        /// See also paper "Paint Inspired Color Mixing and Compositing for Visualization"
        /// </summary>
        public static float[] NextColorAfter(float[] previousColor) {
            var rybColor = RgbToRyb(previousColor);
            return NextColorAfter(rybColor[0], rybColor[1], rybColor[2]);
        }

        /// <summary> This method first separates the red, green, and blue components of the input RGB color.
        /// It then uses these values to calculate the yellow, red, and blue components of the output RYB color.
        /// Finally, it returns the RYB color as a float[] array.
        /// 
        /// Note that this conversion method is based on the RGB-RYB color space conversion algorithm described in the paper
        /// "A Color Space Based on the Red-Yellow-Blue Color Model" by K.E. van der Weij and J.J. van Wijk
        /// (IEEE Transactions on Visualization and Computer Graphics, vol. 15, no. 6, pp. 1453-1458, 2009).
        /// This algorithm is designed to preserve hue and chroma while converting between the RGB and RYB color spaces. </summary>
        public static float[] RgbToRyb(float[] rgb) {
            float r = rgb[0];
            float g = rgb[1];
            float b = rgb[2];
            float minRgb = Math.Min(r, Math.Min(g, b));
            float minRg = Math.Min(r, g);
            float avgRg = (r + g - minRgb) / 2;
            float minRg2 = Math.Min(minRg, avgRg);
            float redNoYellow = r - minRg2;
            float blueNoYellow = b - minRg2;
            float red = minRg2 / (minRg2 + blueNoYellow);
            float blue = blueNoYellow / (minRg2 + blueNoYellow);
            float yellow = minRg2 / (minRg2 + redNoYellow + blueNoYellow);
            return new float[] { red, yellow, blue };
        }

        /// <summary> Mixing random colors with white (255, 255, 255) creates neutral pastels by increasing the
        /// lightness while keeping the hue of the original color </summary>
        public static float[] GetPastelColorVarianfor(float[] inputColor) {
            return MixColors(inputColor, new float[] { 1, 1, 1 });
        }

        public static float[] MixColors(float[] color1, float[] color2) {
            float[] result = new float[3];
            if (color2 != null) {
                result[0] = (int)((color1[0] + color2[0]) / 2);
                result[1] = (int)((color1[1] + color2[1]) / 2);
                result[2] = (int)((color1[2] + color2[2]) / 2);
            }
            return result;
        }

    }

}