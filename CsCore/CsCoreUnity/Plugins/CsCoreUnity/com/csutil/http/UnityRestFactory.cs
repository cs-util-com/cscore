﻿using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    [Obsolete("Use UnityRestFactoryV2 instead", true)]
    public class UnityRestFactory : RestFactory {

        protected override void InitFactory() { }

        public override RestRequest SendRequest(Uri uri, HttpMethod method) {
            return MainThread.instance.ExecuteOnMainThread(() => {
                if (method.ToString() == "GET") {
                    return new UnityRestRequest(UnityWebRequest.Get(uri));
                }
                return new UnityRestRequest(new UnityWebRequest(uri, method.ToString()));
            });
        }

        public override Task<long> GetCurrentPing(string ipOrUrl = DEFAULT_PING_IP, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.WebGLPlayer)) {
                return GetCurrentPingViaHeadRequest(ipOrUrl);
            }
            return base.GetCurrentPing(ipOrUrl, timeoutInMs);
        }

        public Task<long> GetCurrentPingViaHeadRequest(string ipOrUrl = DEFAULT_PING_IP, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
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

        //[Obsolete("UnityEngine.Ping does not exist in WebGL")]
        //private Task<long> GetCurrentPingViaUnity(string ipOrUrl, int timeoutInMs) {
        //    var p = new UnityEngine.Ping(ipOrUrl);
        //    return WebRequestRunner.GetInstance(this).StartCoroutineAsTask<long>(UnityPingCoroutine(p, timeoutInMs), () => {
        //        if (p.isDone) { return p.time; } else { return -1; }
        //    });
        //}

        //[Obsolete("UnityEngine.Ping does not exist in WebGL")]
        //private System.Collections.IEnumerator UnityPingCoroutine(UnityEngine.Ping ping, float timeoutInMs) {
        //    float nrOfChecks = 30;
        //    var wait = new WaitForSeconds(timeoutInMs / nrOfChecks / 1000f);
        //    for (int i = 0; i < nrOfChecks && !ping.isDone; i++) { yield return wait; }
        //}

    }

}
