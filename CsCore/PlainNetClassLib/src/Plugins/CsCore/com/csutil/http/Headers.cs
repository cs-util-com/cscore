using com.csutil.model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.csutil.http {

    public class Headers : IEnumerable<KeyValuePair<string, IEnumerable<string>>> {

        private Dictionary<string, IEnumerable<string>> headers = new Dictionary<string, IEnumerable<string>>();

        public Headers(IEnumerable<KeyValuePair<string, string>> headers) { AddRange(headers); }

        public Headers(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers) { AddRange(headers); }

        public void AddRange(IEnumerable<KeyValuePair<string, string>> headers) {
            foreach (var e in headers) {
                if (!e.Key.IsNullOrEmpty() && !e.Value.IsNullOrEmpty()) {
                    TryAdd(e.Key.ToLowerInvariant(), new string[] { e.Value });
                }
            }
        }

        public void AddRange(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers) {
            foreach (var e in headers) { TryAdd(e.Key.ToLowerInvariant(), e.Value); }
        }

        public void TryAdd(string key, IEnumerable<string> val) {
            try { this.headers.Add(key, val); }
            catch (Exception e) { Log.e($"Could not add header {key}:{val}", e); }
        }

        /// <summary> If null is returned <see cref="Headers.GenerateHashNameFromHeaders"/> could be 
        /// used instead, but this file name would change every time the file changes on the server! </summary>
        public string GetFileNameOnServer(string fallbackValue = null) {
            return ExtractFileName(GetHeaderValue("content-disposition", fallbackValue));
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

        public bool TryGetValue(string key, out IEnumerable<string> value) {
            return headers.TryGetValue(key, out value);
        }

        /// <summary> Returns the MD5 hash in hex (base 16) if available in the headers </summary>
        public string GetMD5Checksum() {
            var md5Hash = GetHeaderValue("content-md5", null);
            if (md5Hash.IsNullOrEmpty()) {  // If the normal md5 header was not found, check for others:
                // Google uses the "x-goog-hash" header instead containing an md5 hash as well:
                if (TryGetXGoogMd5HashInBase64(this, out var xGoogMd5)) { md5Hash = xGoogMd5; }
            }
            if (md5Hash.IsRegexMatch(RegexTemplates.MD5_HASH_BASE64)) {
                md5Hash = BaseConversionHelper.FromBase64StringToHexString(md5Hash);
            }
            return md5Hash;
        }

        private static bool TryGetXGoogMd5HashInBase64(Headers self, out string md5Hash) {
            if (self.TryGetValue("x-goog-hash", out IEnumerable<string> hashes)) {
                md5Hash = hashes.SingleOrDefault(x => x.StartsWith("md5="));
                if (!md5Hash.IsNullOrEmpty()) { // eg "md5=8Uie51Oz+GZcufyQ8q2GwA=="
                    md5Hash = md5Hash.SubstringAfter("md5=");
                    return true;
                }
            }
            md5Hash = null;
            return false;
        }

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
            AssertV3.AreEqual(1, headerValues.Count());
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
            AssertV3.AreEqual(2, x.Length);
            return x.Last();
        }

        public string GetContentMimeType(string fallbackValue) {
            return GetHeaderValue("Content-Type", fallbackValue, allowFuzzyHeaderName: false);
        }

        /// <summary> Returns the number of bytes of data in the body of the response. For a 
        /// file download this might not be the exact size of the transmitted file if the body is
        /// compressed but its an indicator of the total amount of bytes that have to be transfered </summary>
        public int GetContentLengthInBytes(int fallbackValue) {
            return int.Parse(GetHeaderValue("Content-Length", "" + fallbackValue, allowFuzzyHeaderName: false));
        }

        public IEnumerator<KeyValuePair<string, IEnumerable<string>>> GetEnumerator() { return headers.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return headers.GetEnumerator(); }

    }

}