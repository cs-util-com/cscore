using UnityEngine;
using static com.csutil.math.ColorMath;

namespace com.csutil {

    public static class ColorUtil {

        public static Color With(this Color c, float r = -1, float g = -1, float b = -1, float a = -1) {
            return new Color(r < 0 ? c.r : r, g < 0 ? c.g : g, b < 0 ? c.b : b, a < 0 ? c.a : a);
        }

        public static Color WithAlpha(this Color self, float alpha) { return self.With(a: alpha); }

        public static float[] ToHsv(this Color self) { return ToHsv((Color32)self); }
        public static float[] ToHsv(this Color32 self) {
            return RgbToHsv(self.r / 255f, self.g / 255f, self.b / 255f);
        }

        /// <summary> #RRGGBB or #RRGGBBAA string to Color32 object </summary>
        public static Color32 HexStringToColor(string hexString, byte defaultAlpha = 255) {
            if (hexString.StartsWith("#")) { hexString = hexString.Substring(1); }
            var style = System.Globalization.NumberStyles.HexNumber;
            byte r = byte.Parse(hexString.Substring(0, 2), style);
            byte g = byte.Parse(hexString.Substring(2, 2), style);
            byte b = byte.Parse(hexString.Substring(4, 2), style);
            if (hexString.Length >= 8) { defaultAlpha = byte.Parse(hexString.Substring(6, 2), style); }
            return new Color32(r, g, b, defaultAlpha);
        }

        public static Color32 HsvToColor(float hue, float saturation, float value, byte alpha = 255) {
            if (saturation.Equals(0f)) {
                var normV = (byte)(value * 255f);
                return new Color32(normV, normV, normV, alpha);
            }
            var rgb = HsvToRgb(hue, saturation, value);
            return new Color32((byte)(rgb[0] * 255f), (byte)(rgb[1] * 255f), (byte)(rgb[2] * 255f), alpha);
        }

        public static double GetBrightness(this Color self) { return CalcBrightness(self.r, self.g, self.b); }

        public static bool HasGoodContrastTo(this Color self, Color otherColor) {
            return CalcContrastRatio(self.GetBrightness(), otherColor.GetBrightness()) > 4.5;
        }

        public static Color32 GetDarkerVariant(this Color self) { return GetDarkerVariant((Color32)self); }
        public static Color32 GetDarkerVariant(this Color32 self) {
            float[] hsv = self.ToHsv();
            hsv[2] = hsv[2] - 0.3f; // reduce value by 0.3
            if (hsv[2] < 0) { hsv[2] = 0; }
            return HsvToColor(hsv[0], hsv[1], hsv[2]);
        }

        public static Color32 GetComplementaryColor(this Color self) {
            return GetComplementaryColor((Color32)self);
        }
        public static Color32 GetComplementaryColor(this Color32 self) {
            float[] hsv = self.ToHsv();
            InvertHue(hsv);
            return HsvToColor(hsv[0], hsv[1], hsv[2]);
        }

    }

}
