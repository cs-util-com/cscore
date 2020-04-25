using com.csutil.datastructures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class Response<T> {

        public UnityWebRequest request;
        public Action<T> onResult;
        public Action<UnityWebRequest, Exception> onError = (r, e) => { Log.e(e); };
        /// <summary> A value between 0 and 100 </summary>
        public Action<float> onProgress;
        public long maxMsWithoutProgress = 60000;
        public WaitForSeconds wait = new WaitForSeconds(0.05f);
        public ChangeTracker<float> progressInPercent = new ChangeTracker<float>(0);
        public Func<DownloadHandler> createDownloadHandler = NewDefaultDownloadHandler;
        public Func<T> getResult = () => { throw new Exception("Request not yet finished"); };
        public Stopwatch duration;
        public string debugInfo;
        public StackTrace stacktrace = new StackTrace(true);

        public Response<T> WithResultCallback(Action<T> callback) { onResult = callback; return this; }
        public Response<T> WithProgress(Action<float> callback) { onProgress = callback; return this; }

        private static DownloadHandler NewDefaultDownloadHandler() {
            if (typeof(Texture2D).IsCastableTo(typeof(T))) { return new DownloadHandlerTexture(false); }
            return new DownloadHandlerBuffer();
        }

        public override string ToString() {
            var s = debugInfo;
            if (duration != null && !duration.IsRunning) { s += " (" + duration.ElapsedMilliseconds + "ms)"; }
            return s;
        }

    }

}
