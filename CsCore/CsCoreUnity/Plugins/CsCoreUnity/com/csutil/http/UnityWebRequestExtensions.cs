using com.csutil.http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil {

    public static class UnityWebRequestExtensions {

        public static RestRequest SendV2(this UnityWebRequest self) {
            return new UnityRestRequest(self);
        }

        public static IEnumerator SendWebRequestV2<T>(this UnityWebRequest self, Response<T> s) {
            yield return self.SendAndWait(s);
            HandleResult(self, s);
        }

        private static IEnumerator SendAndWait<T>(this UnityWebRequest self, Response<T> resp) {
            var timer = Stopwatch.StartNew();
            try {
                SetupDownloadAndUploadHanders(self, resp);
                resp.duration = Stopwatch.StartNew();
                self.ApplyAllCookiesToRequest();
                resp.debugInfo = self.method + " " + self.url;
                // Log.d("Sending: " + resp);
            }
            catch (Exception ex) { resp.onError(self, ex); throw; }
            var req = self.SendWebRequest();
            timer.AssertUnderXms(40);
            while (!req.isDone) {
                try {
                    var currentProgress = req.progress * 100;
                    if (resp.progressInPercent.SetNewValue(currentProgress)) {
                        timer.Restart();
                        resp.onProgress.InvokeIfNotNull(resp.progressInPercent.value);
                    }
                }
                catch (Exception ex) { resp.onError(self, ex); throw; }
                yield return resp.wait;
                if (timer.ElapsedMilliseconds > resp.maxMsWithoutProgress) { self.Abort(); }
            }
            try {
                resp.duration.Stop();
                // Log.d("   > Finished " + resp);
                AssertResponseLooksNormal(self, resp);
                self.SaveAllNewCookiesFromResponse();
                if (self.error.IsNullOrEmpty()) { resp.progressInPercent.SetNewValue(100); }
                resp.getResult = () => { return self.GetResult<T>(); };
                ProcessServerDate(self.uri, self.GetResponseHeader("date"));
            }
            catch (Exception ex) { resp.onError(self, ex); throw; }
        }

        /// <summary> If available will process and broadcast the received server date </summary>
        /// <param name="utcString"> e.g. "Sun, 08 Mar 2020 09:47:52 GMT" </param>
        private static void ProcessServerDate(Uri uri, string utcString) {
            DateTime? serverUtcDate = null;
            try {
                if (!utcString.IsNullOrEmpty()) {
                    serverUtcDate = DateTimeV2.ParseUtc(utcString);
                }
                if (serverUtcDate.HasValue) {
                    EventBus.instance.Publish(DateTimeV2.SERVER_UTC_DATE, uri, serverUtcDate.Value);
                }
            }
            catch (Exception e) { Log.w("Failed parsing server UTC date: " + e); }
        }

        private static void SetupDownloadAndUploadHanders<T>(UnityWebRequest self, Response<T> resp) {
            self.downloadHandler = resp.createDownloadHandler();
            switch (self.method) {
                case UnityWebRequest.kHttpVerbPUT:
                case UnityWebRequest.kHttpVerbPOST:
                    AssertV2.IsNotNull(self.uploadHandler, "Put/Post-request had no uploadHandler set");
                    break;
            }
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private static void AssertResponseLooksNormal<T>(UnityWebRequest self, Response<T> resp) {
            AssertV2.IsNotNull(self, "WebRequest object was null: " + resp);
            if (self != null) {
                AssertV2.IsTrue(self.isDone, "Request never finished: " + resp);
                if (self.isNetworkError) { Log.w("isNetworkError=true for " + resp); }
                if (self.error != null) { Log.w("error=" + self.error + " for " + resp); }
                if (self.isHttpError) { Log.w("isHttpError=true for " + resp); }
                if (self.responseCode < 200 || self.responseCode >= 300) { Log.w("responseCode=" + self.responseCode + " for " + resp); }
                if (self.isNetworkError && self.responseCode == 0 && self.useHttpContinue) { Log.w("useHttpContinue flag was true, request might work if its set to false"); }
            }
        }

        private static void HandleResult<T>(UnityWebRequest self, Response<T> resp) {
            if (self.isNetworkError || self.isHttpError) {
                resp.onError.InvokeIfNotNull(self, new Exception("[" + self.responseCode + "] " + self.error));
            } else { // .onResult is only informed if there was no network or http error:
                try { resp.onResult?.Invoke(self.GetResult<T>()); }
                catch (Exception e) { resp.onError.InvokeIfNotNull(self, e); }
            }
        }

        public static T GetResult<T>(this UnityWebRequest self) { return self.GetResult<T>(JsonReader.GetReader()); }

        public static T GetResult<T>(this UnityWebRequest self, IJsonReader r) {
            AssertV2.IsTrue(self.isDone, "web request was not done!");
            if (TypeCheck.AreEqual<T, UnityWebRequest>()) { return (T)(object)self; }
            if (typeof(Texture2D).IsCastableTo(typeof(T))) {
                AssertV2.IsTrue(self.downloadHandler is DownloadHandlerTexture,
                    "self.downloadHandler was not a DownloadHandlerTexture but a " + self.downloadHandler.GetType());
                var h = (DownloadHandlerTexture)self.downloadHandler;
                return (T)(object)h.texture;
                //return (T)(object)DownloadHandlerTexture.GetContent(self);
            }
            if (typeof(T).IsCastableTo<Exception>() && self.GetStatusCode().IsErrorStatus()) {
                return (T)(object)new NoSuccessError(self.GetStatusCode(), self.GetResult<string>());
            }
            if (TypeCheck.AreEqual<T, HttpStatusCode>()) { return (T)(object)self.GetStatusCode(); }
            if (TypeCheck.AreEqual<T, Stream>()) { return (T)(object)new MemoryStream(GetBytesResult(self)); }
            if (TypeCheck.AreEqual<T, byte[]>()) { return (T)(object)GetBytesResult(self); }
            if (TypeCheck.AreEqual<T, Headers>()) { return (T)(object)self.GetResponseHeadersV2(); }
            var text = GetStringResult(self);
            if (TypeCheck.AreEqual<T, string>()) { return (T)(object)text; }
            return r.Read<T>(text);
        }

        private static byte[] GetBytesResult(UnityWebRequest self) {
            if (ResponseIsGZipped(self)) {
                try { return DecompressGzip(self.downloadHandler?.data); }
                catch (Exception e) { Log.e(e); }
            }
            return self.downloadHandler.data;
        }

        private static string GetStringResult(UnityWebRequest self) {
            if (ResponseIsGZipped(self)) {
                try { return Encoding.UTF8.GetString(DecompressGzip(self.downloadHandler?.data)); }
                catch (Exception e) { Log.d("Failed to decompress gzip, will fallback to ww.text", e); }
            }
            return self.downloadHandler?.text;
        }

        private static bool ResponseIsGZipped(UnityWebRequest self) {
            return self.GetResponseHeader("content-encoding")?.ToLowerInvariant() == "gzip";
        }

        private static byte[] DecompressGzip(byte[] gzippedData) {

            using (var stream = new MemoryStream(gzippedData)) {
                using (var gzipStream = new GZipStream(stream, CompressionMode.Decompress, leaveOpen: true)) {
                    return gzipStream.ToByteArray();
                }
            }
        }

        public static HttpStatusCode GetStatusCode(this UnityWebRequest self) {
            return (HttpStatusCode)self.responseCode;
        }

        public static UnityWebRequest SetRequestHeaders(this UnityWebRequest self, IEnumerable<KeyValuePair<string, IEnumerable<string>>> headersToAdd) {
            if (!headersToAdd.IsNullOrEmpty()) {
                foreach (var h in headersToAdd) {
                    AssertV2.AreEqual(1, h.Value.Count());
                    self.SetRequestHeader(h.Key, h.Value.First());
                }
            }
            return self;
        }

        public static Headers GetResponseHeadersV2(this UnityWebRequest self) {
            return new Headers(self.GetResponseHeaders());
        }

    }

}
