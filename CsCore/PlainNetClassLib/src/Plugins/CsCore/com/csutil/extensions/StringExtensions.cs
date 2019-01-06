using System;

namespace com.csutil {

    public static class StringExtensions {

        /// <summary> "abcd".Substring("bc") == "abc"   AND   "abcd".Substring("bc", false) == "a"</summary>
        public static string Substring(this string self, string end, bool includeEnd = true) { return Substring(self, 0, end, includeEnd); }

        /// <summary> "abcd".Substring(0, "bc") == "abc"   AND   "abcd".Substring(0, "bc", false) == "a"</summary>
        public static string Substring(this string self, int startIndex, string end, bool includeEnd = true) {
            var lengthUntilEndStarts = self.LastIndexOf(end);
            if (lengthUntilEndStarts < 0) { return self; }
            var lengthOfEnd = (includeEnd ? end.Length : 0);
            return self.Substring(startIndex, lengthUntilEndStarts + lengthOfEnd - startIndex);
        }

        public static string SubstringAfter(this string self, string startAfter, bool startFromBack = false) {
            var pos = startFromBack ? self.LastIndexOf(startAfter) : self.IndexOf(startAfter);
            if (pos < 0) { return ""; }
            return self.Substring(pos + startAfter.Length);
        }

        public static bool EndsWith(this string self, char end) { return self.EndsWith("" + end); }

        public static string[] Split(this string self, string separator) {
            return self.Split(new string[] { separator }, StringSplitOptions.None);
        }

     }

}
