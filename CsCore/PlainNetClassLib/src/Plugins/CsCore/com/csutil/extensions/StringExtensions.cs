using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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

        public static string[] Split(this string self, string separator) {
            return self.Split(new string[] { separator }, StringSplitOptions.None);
        }

        /// <summary>
        /// Take any string and encrypt it using SHA1 then return the encrypted data
        /// </summary>
        /// <param name="data">input text you will enterd to encrypt it</param>
        /// <returns>return the encrypted text as hexadecimal string</returns>
        public static string GetSHA1Hash(this string data) {
            if (data == null) { Log.e("GetSHA1Hash: passed string is null"); return null; }
            string strResult = string.Empty;
            SHA1CryptoServiceProvider sha1Obj = new SHA1CryptoServiceProvider();
            byte[] bytesToHash = Encoding.ASCII.GetBytes(data);
            bytesToHash = sha1Obj.ComputeHash(bytesToHash);
            foreach (Byte b in bytesToHash) {
                strResult += b.ToString("x2");
            }
            return strResult;
        }

    }

}
