using System;

namespace com.csutil.math {

    public static class ColorMath {

        /// <summary> 
        /// Calculates the brightness using the relative luminanace:
        /// The relative brightness of any point in a colorspace, 
        /// normalized to 0 for darkest black and 1 for lightest white
        /// see http://www.w3.org/TR/2008/REC-WCAG20-20081211/#relativeluminancedef 
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
        /// and http://www.w3.org/TR/2008/REC-WCAG20-20081211/#contrast-ratiodef 
        /// </summary>
        /// <param name="brightness1"> use CalcBrightness(r,g,b) </param>
        /// <param name="brightness2"> use CalcBrightness(r,g,b) </param>
        /// <returns> a ratio > 4.5 is considered a "good" contrast </returns>
        public static double CalcContrastRatio(double brightness1, double brightness2) {
            if (brightness1 > brightness2) { return (brightness1 + 0.05) / (brightness2 + 0.05); }
            return (brightness2 + 0.05) / (brightness1 + 0.05);
        }

        /// <summary> Can be used to invert the color to its complimentary version </summary>
        public static void InvertHue(float[] hsv) { hsv[0] = (hsv[0] + 180) % 360; }

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
            return new float[3] { r, g, b };
        }

    }

}