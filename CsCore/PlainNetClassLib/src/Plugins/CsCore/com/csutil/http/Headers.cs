using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.http {

    public class Headers : IEnumerable<KeyValuePair<string, IEnumerable<string>>> {

        private Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();

        public Headers(IEnumerable<KeyValuePair<string, string>> headers) {
            foreach (var e in headers) { this.headers.Add(e.Key.ToLowerInvariant(), new string[] { e.Value }); }
        }

        public Headers(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers) {
            foreach (var e in headers) { this.headers.Add(e.Key.ToLowerInvariant(), e.Value); }
        }

        public string GetFileNameOnServer() {
            return ExtractFileName(GetHeaderValue("content-disposition", null));
        }

        public string GenerateHashNameFromHeaders() {
            var generatedName = CombineHeadersToName();
            if (generatedName.IsNullOrEmpty()) {
                Log.w("None of the headers were used to generate a file name: " + this.ToString());
                return null;
            }
            generatedName = generatedName.GetMD5Hash();
            string ext = GetFileExtensionFromMimeType(null);
            if (ext != null) { generatedName += "." + ext; }
            return generatedName;
        }

        private string CombineHeadersToName() {
            Log.w("Filename not found, will fallback to hash+mimetype");
            if (!GetEtagHeader().IsNullOrEmpty()) { return GetEtagHeader(); }
            if (!GetMD5Checksum().IsNullOrEmpty()) { return GetMD5Checksum(); }
            return GetHeaderValue("Last-Modified", "") + GetHeaderValue("Content-Length", "") + GetContentMimeType("");
        }

        public string GetMD5Checksum() { return GetHeaderValue("content-md5", null); }

        public string GetEtagHeader() { return GetHeaderValue("etag", null); }

        public long GetFileSizeInBytesOnServer() { return long.Parse(GetHeaderValue("Content-Length", "-1")); }

        public override string ToString() {
            if (headers.IsNullOrEmpty()) { return "(Emtpy Headers object)"; }
            var s = "Headers: ((";
            foreach (var e in headers) { s += "\n  > " + e.Key + " ->  " + e.Value.ToStringV2(); }
            return s + "/n))";
        }

        public DateTime GetLastModifiedUtcDate(DateTime fallbackUtcTime) {
            string dateString = GetRawLastModifiedString();
            try { if (dateString != null) { return DateTimeV2.ParseUtc(dateString); } } catch { }
            return fallbackUtcTime;
        }

        public string GetRawLastModifiedString() { return GetHeaderValue("last-modified", null); }

        public IEnumerable<string> GetHeaderValues(string headerName) {
            return headers.GetValue(headerName.ToLowerInvariant(), null);
        }

        public string GetHeaderValue(string headerName, string fallbackValue, bool allowFuzzyHeaderName = true) {
            var headerValues = GetHeaderValues(headerName);
            if (headerValues == null && allowFuzzyHeaderName) {
                // Google names it "x-goog-stored-content-length" instead of "content-length"
                var similar = headers.Filter(x => x.Key.Contains(headerName.ToLowerInvariant()));
                // If there is exactly 1 header that matches the fuzzy search, use that one:
                if (similar.Count() == 1) { headerValues = similar.First().Value; }
            }
            if (headerValues == null) { return fallbackValue; }
            AssertV2.AreEqual(1, headerValues.Count());
            return headerValues.First();
        }

        private static string ExtractFileName(string headerWithFilename) {
            if (headerWithFilename == null) { return null; }
            var words = headerWithFilename.Split(';');
            foreach (var word in words) {
                var pair = headerWithFilename.Split('=');
                if (pair != null && pair[0].IndexOf("filename") >= 0) { return pair[1].Trim(); }
            }
            return null;
        }

        private string GetFileExtensionFromMimeType(string fallbackValue) {
            var mime = GetContentMimeType(fallbackValue);
            if (mime == null) { return fallbackValue; }
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Type
            if (mime.Contains(";")) { mime = mime.Split(";").First(p => p.Contains("/")); }
            var x = mime.Split("/");
            AssertV2.AreEqual(2, x.Length);
            return x.Last();
        }

        public string GetContentMimeType(string fallbackValue) {
            return GetHeaderValue("Content-Type", fallbackValue, allowFuzzyHeaderName: false);
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator() { return headers.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return headers.GetEnumerator(); }
    }

}