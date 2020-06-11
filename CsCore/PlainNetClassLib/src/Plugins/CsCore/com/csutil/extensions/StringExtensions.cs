using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace com.csutil {

    public static class StringExtensions {

        /// <summary> 
        /// Examples: 
        /// <para> "abc)]".Substring(")", includeEnd: false) == "abc"   </para>
        /// <para> AND                                                  </para>
        /// <para> "abc)]".Substring("bc", includeEnd: true) == "abc"   </para>
        /// </summary>
        public static string Substring(this string self, string end, bool includeEnd) { return Substring(self, 0, end, includeEnd); }

        /// <summary> 
        /// Examples: 
        /// <para> "[(abc)]".Substring(2, ")", includeEnd: false) == "abc"  </para>
        /// <para> AND                                                      </para>
        /// <para> "abc)]".Substring(2, "bc", includeEnd: true) == "abc"    </para>
        /// </summary>
        public static string Substring(this string self, int startIndex, string end, bool includeEnd) {
            var lengthUntilEndStarts = self.LastIndexOf(end);
            if (lengthUntilEndStarts < 0) { return self.Substring(startIndex); }
            var lengthOfEnd = (includeEnd ? end.Length : 0);
            var length = lengthUntilEndStarts + lengthOfEnd - startIndex;
            if (length < 0) { return self.Substring(startIndex); }
            return self.Substring(startIndex, length);
        }

        public static string SubstringAfter(this string self, string startAfter, bool startFromBack = false) {
            var pos = startFromBack ? self.LastIndexOf(startAfter) : self.IndexOf(startAfter);
            if (pos < 0) { throw new IndexOutOfRangeException("Substring " + startAfter + " not found in " + self); }
            return self.Substring(pos + startAfter.Length);
        }

        public static bool EndsWith(this string self, char end) { return self.EndsWith("" + end); }

        public static string[] Split(this string self, string separator) {
            return self.Split(new string[] { separator }, StringSplitOptions.None);
        }

        /// <summary> "An {0} with {1} placeholders!".With("example", "multiple") </summary>
        public static string With(this string self, params object[] args) {
            return string.Format(self, args);
        }

        /// <summary>
        /// Examples: 
        /// <para> Assert.True(myUrl1.IsRegexMatch(RegexTemplates.URL)); </para>
        /// <para> Assert.True("abc".IsRegexMatch("a*"));                </para>
        /// <para> Assert.True("Abc".IsRegexMatch("[A-Z][a-z][a-z]"));   </para>
        /// <para> Assert.True("hat".IsRegexMatch("?at"));               </para>
        /// <para> Assert.True("joe".IsRegexMatch("[!aeiou]*"));         </para>
        /// <para> Assert.False("joe".IsRegexMatch("?at"));              </para>
        /// <para> Assert.False("joe".IsRegexMatch("[A-Z][a-z][a-z]"));  </para>
        /// </summary>
        public static bool IsRegexMatch(this string self, string regexToMatch) {
            if (self == null) { return false; }
            if (regexToMatch.IsNullOrEmpty()) { throw new ArgumentException($"Invalid regexToMatch '{regexToMatch}'"); };
            try { return Regex.IsMatch(self, regexToMatch); } catch (ArgumentException e) {
                throw new ArgumentException("Invalid pattern: " + regexToMatch, e);
            }
        }

    }

    public static class ByteSizeToString {

        private static readonly KeyValuePair<long, string>[] thresholds ={
            new KeyValuePair<long, string>(1, " Byte"),
            new KeyValuePair<long, string>(2, " Bytes"),
            new KeyValuePair<long, string>(1024, " KB"),
            new KeyValuePair<long, string>(1048576, " MB"), // Note: 1024 ^ 2 = 1026 (xor operator)
            new KeyValuePair<long, string>(1073741824, " GB")
        };

        public static string ByteSizeToReadableString(long value) {
            if (value == 0) { return "0 Bytes"; }
            for (int t = thresholds.Length - 1; t > 0; t--) {
                if (value >= thresholds[t].Key) {
                    return ((double)value / thresholds[t].Key).ToString("0.00") + thresholds[t].Value;
                }
            }
            return "-" + ByteSizeToReadableString(-value); // negative bytes (common case optimised to the end of this routine)
        }

        public static string ToFirstCharUpperCase(this string self) {
            if (self.IsNullOrEmpty()) { return self; }
            return self.Substring(0, 1).ToUpperInvariant() + self.Substring(1);
        }

        public static string ToFirstCharLowerCase(this string self) {
            if (self.IsNullOrEmpty()) { return self; }
            return self.Substring(0, 1).ToLowerInvariant() + self.Substring(1);
        }

    }

}
