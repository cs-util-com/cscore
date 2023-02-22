using System.Collections.Generic;
using System.Linq;
using com.csutil.math;
using UnityEngine;
using Random = System.Random;

namespace com.csutil {

    public static class ColorUtil {

        /// <summary> All channels must be between 0 and 255 </summary>
        public static Color32 With(this Color32 c, int r = -1, int g = -1, int b = -1, int a = -1) {
            return new Color32(r < 0 ? c.r : (byte)r, g < 0 ? c.g : (byte)g, b < 0 ? c.b : (byte)b, a < 0 ? c.a : (byte)a);
        }

        /// <summary> All channels must be between 0 and 1 </summary>
        public static Color With(this Color c, float r = -1, float g = -1, float b = -1, float a = -1) {
            return new Color(r < 0 ? c.r : r, g < 0 ? c.g : g, b < 0 ? c.b : b, a < 0 ? c.a : a);
        }

        /// <summary> Takes an alpha value between 0 and 1 and returns a new color with this alpha </summary>
        public static Color WithAlpha(this Color self, float alpha0To1) { return self.With(a: alpha0To1); }

        public static Color32 WithAlpha(this Color32 self, byte alpha0To255) { return self.With(a: alpha0To255); }

        /// <summary> Returns HSV (hue 0-360 (color), saturation 0-1, value 0-1 (brightness) </summary>
        public static float[] ToHsv(this Color self) { return ToHsv((Color32)self); }

        /// <summary> Returns HSV (hue 0-360 (color), saturation 0-1, value 0-1 (brightness) </summary>
        public static float[] ToHsv(this Color32 self) {
            // The channels of Color32 are from 0 to 255 (not like Color where its 0 to 1)
            return ColorMath.RgbToHsv(self.r / 255f, self.g / 255f, self.b / 255f);
        }

        /// <summary> #RRGGBB or #RRGGBBAA string to Color32 object </summary>
        public static Color32 HexStringToColor(string hexString, byte defaultAlpha0To255 = 255) {
            if (hexString.StartsWith("#")) { hexString = hexString.Substring(1); }
            var style = System.Globalization.NumberStyles.HexNumber;
            byte r = byte.Parse(hexString.Substring(0, 2), style);
            byte g = byte.Parse(hexString.Substring(2, 2), style);
            byte b = byte.Parse(hexString.Substring(4, 2), style);
            if (hexString.Length >= 8) { defaultAlpha0To255 = byte.Parse(hexString.Substring(6, 2), style); }
            return new Color32(r, g, b, defaultAlpha0To255);
        }

        public static Color HsvToColor(float[] hsv) { return HsvToColor32(hsv); }

        public static Color HsvToColor(float hue0To360, float saturation0To1, float value0To1, float alpha0To1 = 1) {
            return HsvToColor32(hue0To360, saturation0To1, value0To1, (byte)(alpha0To1 * 255));
        }

        public static Color32 HsvToColor32(float[] hsv) { return HsvToColor32(hsv[0], hsv[1], hsv[2]); }

        /// <summary> Converts hsv values to Color32 values (range 0 to 255) </summary>
        /// <param name="hue0To360"> From 0 (red) to 360 (also red, wraps around) </param>
        /// <param name="saturation0To1"> From 0 to 1 </param>
        /// <param name="value0To1"> From 0 to 1 </param>
        /// <param name="alpha0To255"> From 0 to 255 </param>
        public static Color32 HsvToColor32(float hue0To360, float saturation0To1, float value0To1, byte alpha0To255 = 255) {
            if (saturation0To1.Equals(0f)) {
                var normV = (byte)(value0To1 * 255f);
                return new Color32(normV, normV, normV, alpha0To255);
            }
            float[] rgb = ColorMath.HsvToRgb(hue0To360, saturation0To1, value0To1);
            return ToColor32(rgb, alpha0To255);
        }

        private static Color32 ToColor32(float[] rgb, byte alpha0To255 = 255) {
            return new Color32((byte)(rgb[0] * 255), (byte)(rgb[1] * 255), (byte)(rgb[2] * 255), alpha0To255);
        }

        private static float[] ToFloatArray(this Color32 c) {
            return new float[] { c.r / 255f, c.g / 255f, c.b / 255f };
        }

        public static double GetBrightness(this Color self) {
            return ColorMath.CalcBrightness(self.r, self.g, self.b);
        }

        public static double GetBrightness(this Color32 self) {
            return ColorMath.CalcBrightness(self.r / 255d, self.g / 255d, self.b / 255d);
        }

        public static bool HasGoodContrastTo(this Color self, Color otherColor) {
            return ColorMath.CalcContrastRatio(self.GetBrightness(), otherColor.GetBrightness()) > 4.5;
        }

        public static Color GetContrastBlackOrWhite(this Color self) {
            return ((Color32)self).GetContrastBlackOrWhite();
        }

        public static Color32 GetContrastBlackOrWhite(this Color32 self) {
            if (Color.white.HasGoodContrastTo(self)) { return Color.white; }
            return Color.black;
        }

        public static Color GetDarkerVariant(this Color self) { return GetDarkerVariant((Color32)self); }
        public static Color32 GetDarkerVariant(this Color32 self) {
            float[] hsv = self.ToHsv();
            hsv[2] = hsv[2] - 0.3f; // reduce value by 0.3
            if (hsv[2] < 0) { hsv[2] = 0; }
            return HsvToColor32(hsv);
        }

        public static Color GetComplementaryColor(this Color self) {
            return GetComplementaryColor((Color32)self);
        }

        public static Color32 GetComplementaryColor(this Color32 self) {
            float[] hsv = self.ToHsv();
            ColorMath.InvertHue(hsv);
            return HsvToColor32(hsv);
        }

        public static Queue<Color32> NextRandomColors(this Random self, int count, byte alpha0To255 = 255, float range = 4f) {
            ISet<float[]> colors = self.NextRandomRgbColors(count, range);
            return colors.Map(c => ToColor32(c, alpha0To255)).ToQueue();
        }

        public static IEnumerable<Color32> GetPastelColorVariantFor(this IEnumerable<Color32> self, float whiteAmount = 1) {
            return self.Map(c => ToColor32(ColorMath.GetPastelColorVariantFor(c.ToFloatArray(), whiteAmount)));
        }

    }

}