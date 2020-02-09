using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestFactory : RestFactory {

        public override RestRequest SendRequest(Uri uri, HttpMethod method) {
            if (method.ToString() == "GET") {
                return new UnityRestRequest(UnityWebRequest.Get(uri));
            }
            return new UnityRestRequest(new UnityWebRequest(uri, method.ToString()));
        }

        public override async Task<long> GetCurrentPing(string ipOrUrl = "8.8.8.8", int timeoutInMs = 500) {
            if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.WebGLPlayer)) {
                var t1 = GetCurrentPingViaHeadRequest(ipOrUrl);
                var t2 = GetCurrentPingViaUnity(ipOrUrl, timeoutInMs);
                var quickerTask = await Task.WhenAny(t1, t2);
                return await quickerTask;
            }
            // For Android and MacOs the Unity.Ping seems to not work for all types of ips and urls
            return await base.GetCurrentPing(ipOrUrl, timeoutInMs);
        }

        public Task<long> GetCurrentPingViaHeadRequest(string ipOrUrl = "8.8.8.8", int timeoutInMs = 500) {
            Stopwatch t = Stopwatch.StartNew();
            return WebRequestRunner.GetInstance(this).StartCoroutineAsTask(SendHeadReqTo(ipOrUrl), () => {
                t.Stop();
                return t.ElapsedMilliseconds;
            }).WithTimeout(timeoutInMs);
        }

        private System.Collections.IEnumerator SendHeadReqTo(string ipOrUrl) {
            Response<UnityWebRequest> resp = new Response<UnityWebRequest>();
            resp.onError = null;
            if (!ipOrUrl.StartsWith("http")) { ipOrUrl = "https://" + ipOrUrl; }
            yield return UnityWebRequest.Head(ipOrUrl).SendWebRequestV2(resp);
        }

        [Obsolete("UnityEngine.Ping does not exist in WebGL")]
        private Task<long> GetCurrentPingViaUnity(string ipOrUrl, int timeoutInMs) {
            var p = new UnityEngine.Ping(ipOrUrl);
            return WebRequestRunner.GetInstance(this).StartCoroutineAsTask<long>(UnityPingCoroutine(p, timeoutInMs), () => {
                if (p.isDone) { return p.time; } else { return -1; }
            });
        }

        [Obsolete("UnityEngine.Ping does not exist in WebGL")]
        private System.Collections.IEnumerator UnityPingCoroutine(UnityEngine.Ping ping, float timeoutInMs) {
            float nrOfChecks = 30;
            var wait = new WaitForSeconds(timeoutInMs / nrOfChecks / 1000f);
            for (int i = 0; i < nrOfChecks && !ping.isDone; i++) { yield return wait; }
        }

    }

}
