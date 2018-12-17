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

        private static IEnumerator SendV2<T>(this UnityWebRequest self, Action<T> onResult, Action<UnityWebRequest, Exception> onError, Action<float> onProgress = null, long maxMsWithoutProgress = 60000) {

            if (self.downloadHandler == null && onResult != null) { self.downloadHandler = newDefaultDownloadHandler<T>(); }

            var req = self.SendWebRequest();
            var wait = new WaitForSeconds(0.05f);
            var timer = Stopwatch.StartNew();
            var progress = new ChangeTracker<float>(0);
            while (!req.isDone) {
                if (progress.set(req.progress)) {
                    timer.Restart();
                    onProgress.InvokeIfNotNull(progress.value);
                }
                yield return wait;
                if (timer.ElapsedMilliseconds > maxMsWithoutProgress) { self.Abort(); }
            }

            try {
                if (self.isNetworkError || self.isHttpError) {
                    throw new Exception(self.error);
                } else {
                    onResult.InvokeIfNotNull(self.GetResult<T>());
                }
            }
            catch (Exception e) { if (!onError.InvokeIfNotNull(self, e)) { Log.e(e); }; }

            yield return null;
        }


        private static T GetResult<T>(this UnityWebRequest self) { return self.GetResult<T>(JsonReader.NewReader()); }

        private static T GetResult<T>(this UnityWebRequest self, IJsonReader r) {
            AssertV2.IsTrue(self.isDone, "web request was not done!");
            if (TypeCheck.AreEqual<T, UnityWebRequest>()) { return (T)(object)self; }
            if (typeof(Texture).IsAssignableFrom<T>()) { return (T)(object)((DownloadHandlerTexture)self.downloadHandler).texture; }
            var text = self.downloadHandler.text;
            if (TypeCheck.AreEqual<T, String>()) { return (T)(object)text; }
            return r.Read<T>(text);
        }

        private class ChangeTracker<T> {
            public T value { get; private set; }
            public ChangeTracker(T t) { value = t; }
            public bool set(T t) { if (Equals(t, value)) { return false; } value = t; return true; }
        }

        private static DownloadHandler newDefaultDownloadHandler<T>() {
            if (typeof(Texture2D).IsAssignableFrom<T>()) { return new DownloadHandlerTexture(false); }
            return new DownloadHandlerBuffer();
        }

    }

}
