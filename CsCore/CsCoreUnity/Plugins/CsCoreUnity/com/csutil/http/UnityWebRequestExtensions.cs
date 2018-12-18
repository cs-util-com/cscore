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

namespace UnityEngine.Networking {

    public static class UnityWebRequestExtensions {

        public static RestRequest SendV2(this UnityWebRequest self) { return new UnityRestRequest(self); }

        public static IEnumerator SendWebRequestV2<T>(this UnityWebRequest self, Response<T> s) {
            yield return self.SendAndWait(s);
            HandleResult(self, s);
        }

        private static IEnumerator SendAndWait<T>(this UnityWebRequest self, Response<T> s) {
            if (self.downloadHandler == null && s.onResult != null) { self.downloadHandler = s.createDownloadHandler(); }
            s.duration = Stopwatch.StartNew();
            var timer = Stopwatch.StartNew();
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
            if (self.error.IsNullOrEmpty()) { s.progressInPercent.setNewValue(100); }
            s.getResult = () => { return self.GetResult<T>(); };
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
            if (typeof(Texture).IsAssignableFrom<T>()) {
                AssertV2.IsTrue(self.downloadHandler is DownloadHandlerTexture, "self.downloadHandler was not a DownloadHandlerTexture");
                var h = (DownloadHandlerTexture)self.downloadHandler;
                return (T)(object)h.texture;
            }
            var text = self.downloadHandler.text;
            if (TypeCheck.AreEqual<T, String>()) { return (T)(object)text; }
            return r.Read<T>(text);
        }

    }

}
