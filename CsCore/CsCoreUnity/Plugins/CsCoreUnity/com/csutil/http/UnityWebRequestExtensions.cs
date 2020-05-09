using com.csutil.http;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                if (self.downloadHandler == null) { self.downloadHandler = resp.createDownloadHandler(); }
                resp.debugInfo = self.method + " " + self.url + " with cookies=[" + self.GetRequestHeader("Cookie") + "]";
                Log.d("Sending: " + resp);
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
                Log.d("   > Finished " + resp);
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
            try { if (!utcString.IsNullOrEmpty()) { serverUtcDate = DateTimeV2.ParseUtc(utcString); } }
            catch (Exception e) { Log.w("Failed parsing server UTC date: " + e); }
            if (serverUtcDate.HasValue) { EventBus.instance.Publish(DateTimeV2.SERVER_UTC_DATE, uri, serverUtcDate.Value); }
        }

        private static void SetupDownloadAndUploadHanders<T>(UnityWebRequest self, Response<T> resp) {
            if (self.downloadHandler == null && resp.onResult != null) { self.downloadHandler = resp.createDownloadHandler(); }
            switch (self.method) {
                case UnityWebRequest.kHttpVerbGET:
                    AssertV2.IsNotNull(self.downloadHandler, "Get-request had no downloadHandler set");
                    break;
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
            } else {
                try {
                    if (resp.onResult != null) { resp.onResult(self.GetResult<T>()); } else {
                        Log.d("resp.onResult was null, resp.GetResult has to be called manually");
                    }
                }
                catch (Exception e) { resp.onError.InvokeIfNotNull(self, e); }
            }
        }

        public static T GetResult<T>(this UnityWebRequest self) { return self.GetResult<T>(JsonReader.GetReader()); }

        public static T GetResult<T>(this UnityWebRequest self, IJsonReader r) {
            AssertV2.IsTrue(self.isDone, "web request was not done!");
            if (TypeCheck.AreEqual<T, UnityWebRequest>()) { return (T)(object)self; }
            if (typeof(Texture2D).IsCastableTo(typeof(T))) {
                AssertV2.IsTrue(self.downloadHandler is DownloadHandlerTexture, "self.downloadHandler was not a DownloadHandlerTexture");
                var h = (DownloadHandlerTexture)self.downloadHandler;
                return (T)(object)h.texture;
            }
            if (TypeCheck.AreEqual<T, Stream>()) { return (T)(object)new MemoryStream(self.downloadHandler.data); }
            if (TypeCheck.AreEqual<T, byte[]>()) { return (T)(object)self.downloadHandler.data; }
            if (TypeCheck.AreEqual<T, Headers>()) { return (T)(object)self.GetResponseHeadersV2(); }
            var text = self.downloadHandler.text;
            if (TypeCheck.AreEqual<T, string>()) { return (T)(object)text; }
            return r.Read<T>(text);
        }

        public static UnityWebRequest SetRequestHeaders(this UnityWebRequest self, Headers headersToAdd) {
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
