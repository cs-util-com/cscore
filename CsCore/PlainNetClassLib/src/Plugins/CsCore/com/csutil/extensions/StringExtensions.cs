using System;

namespace com.csutil {

    public static class StringExtensions {

        /// <summary> 
        /// "abc)]".Substring(")", includeEnd: false) == "abc"
        /// AND
        /// "abc)]".Substring("bc", includeEnd: true) == "abc" 
        /// </summary>
        public static string Substring(this string self, string end, bool includeEnd) { return Substring(self, 0, end, includeEnd); }

        /// <summary> 
        /// "[(abc)]".Substring(2, ")", includeEnd: false) == "abc"
        /// AND
        /// "abc)]".Substring(2, "bc", includeEnd: true) == "abc" 
        /// </summary>
        public static string Substring(this string self, int startIndex, string end, bool includeEnd) {
            var lengthUntilEndStarts = self.LastIndexOf(end);
            if (lengthUntilEndStarts < 0) { return self.Substring(startIndex); }
            var lengthOfEnd = (includeEnd ? end.Length : 0);
            return self.Substring(startIndex, lengthUntilEndStarts + lengthOfEnd - startIndex);
        }

        public static string SubstringAfter(this string self, string startAfter, bool startFromBack = false) {
            var pos = startFromBack ? self.LastIndexOf(startAfter) : self.IndexOf(startAfter);
            if (pos < 0) { throw Log.e("Substring " + startAfter + " not found in " + self); }
            return self.Substring(pos + startAfter.Length);
        }

        public static bool EndsWith(this string self, char end) { return self.EndsWith("" + end); }

        public static string[] Split(this string self, string separator) {
            return self.Split(new string[] { separator }, StringSplitOptions.None);
        }

    }

}
