using com.csutil;
using com.csutil.http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil {

    public static class UnityWebRequestExtensions {

        public static RestRequest SendV2(this UnityWebRequest self) { return new UnityRestRequest(self); }

        public static IEnumerator SendWebRequestV2<T>(this UnityWebRequest self, Response<T> s) {
            yield return self.SendAndWait(s);
            HandleResult(self, s);
        }

        private static IEnumerator SendAndWait<T>(this UnityWebRequest self, Response<T> s) {
            s.debugInfo = self.method + " " + self.url;
            SetupDownloadAndUploadHanders(self, s);
            s.duration = Stopwatch.StartNew();
            var timer = Stopwatch.StartNew();
            self.ApplyAllCookiesToRequest();
            var req = self.SendWebRequest();
            AssertV2.IsTrue(timer.ElapsedMilliseconds < 10, "timer.ElapsedMilliseconds already at " + timer.ElapsedMilliseconds + " ms");
            while (!req.isDone) {
                if (s.progressInPercent.setNewValue(req.progress * 100)) {
                    timer.Restart();
                    s.onProgress.InvokeIfNotNull(s.progressInPercent.value);
                }
                yield return s.wait;
                if (timer.ElapsedMilliseconds > s.maxMsWithoutProgress) { self.Abort(); }
            }
            s.duration.Stop();
            AssertResponseLooksNormal(self, s);
            self.SaveAllNewCookiesFromResponse();
            if (self.error.IsNullOrEmpty()) { s.progressInPercent.setNewValue(100); }
            s.getResult = () => { return self.GetResult<T>(); };
        }

        private static void SetupDownloadAndUploadHanders<T>(UnityWebRequest self, Response<T> s) {
            switch (self.method) {
                case UnityWebRequest.kHttpVerbGET:
                    AssertV2.IsNotNull(self.downloadHandler, "Get-request had no downloadHandler set");
                    break;
                case UnityWebRequest.kHttpVerbPUT:
                case UnityWebRequest.kHttpVerbPOST:
                    AssertV2.IsNotNull(self.uploadHandler, "Put/Post-request had no uploadHandler set");
                    break;
            }
            if (self.downloadHandler == null && s.onResult != null) { self.downloadHandler = s.createDownloadHandler(); }
        }

        [Conditional("DEBUG")]
        private static void AssertResponseLooksNormal<T>(UnityWebRequest self, Response<T> s) {
            AssertV2.IsNotNull(self, "WebRequest object was null: " + s);
            if (self != null) {
                AssertV2.IsTrue(self.isDone, "Request never finished: " + s);
                if (self.isNetworkError) { Log.w("isNetworkError=true for " + s); }
                if (self.error != null) { Log.w("error=" + self.error + " for " + s); }
                if (self.isHttpError) { Log.w("isHttpError=true for " + s); }
                if (self.responseCode < 200 || self.responseCode >= 300) { Log.w("responseCode=" + self.responseCode + " for " + s); }
                if (self.isNetworkError && self.responseCode == 0 && self.useHttpContinue) { Log.w("useHttpContinue flag was true, request might work if its set to false"); }
            }
        }

        private static void HandleResult<T>(UnityWebRequest self, Response<T> s) {
            if (self.isNetworkError || self.isHttpError) {
                s.onError(self, new Exception(self.error));
            } else {
                try { s.onResult.InvokeIfNotNull(self.GetResult<T>()); } catch (Exception e) { s.onError(self, e); }
            }
        }

        public static T GetResult<T>(this UnityWebRequest self) { return self.GetResult<T>(JsonReader.NewReader()); }

        public static T GetResult<T>(this UnityWebRequest self, IJsonReader r) {
            AssertV2.IsTrue(self.isDone, "web request was not done!");
            if (TypeCheck.AreEqual<T, UnityWebRequest>()) { return (T)(object)self; }
            if (typeof(Texture2D).IsCastableTo(typeof(T))) {
                AssertV2.IsTrue(self.downloadHandler is DownloadHandlerTexture, "self.downloadHandler was not a DownloadHandlerTexture");
                var h = (DownloadHandlerTexture)self.downloadHandler;
                return (T)(object)h.texture;
            }
            if (TypeCheck.AreEqual<T, Headers>()) { return (T)(object)new Headers(self.GetResponseHeaders()); }
            var text = self.downloadHandler.text;
            if (TypeCheck.AreEqual<T, String>()) { return (T)(object)text; }
            return r.Read<T>(text);
        }

    }

}
