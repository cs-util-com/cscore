using System;
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

        //public override Task<long> GetCurrentPing(string ipOrUrl = "8.8.8.8", int timeoutInMs = 500) {
        //    if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.Android, RuntimePlatform.OSXPlayer, RuntimePlatform.WindowsPlayer)) {
        //        // For Android and MacOs the Unity.Ping seems to not work for all types of ips and urls
        //        return base.GetCurrentPing(ipOrUrl, timeoutInMs);
        //    }
        //    return GetCurrentPingViaUnity(ipOrUrl, timeoutInMs);
        //}

        //private Task<long> GetCurrentPingViaUnity(string ipOrUrl, int timeoutInMs) {
        //    var p = new UnityEngine.Ping(ipOrUrl);
        //    return WebRequestRunner.GetInstance(this).StartCoroutineAsTask<long>(UnityPingCoroutine(p, timeoutInMs), () => {
        //        if (p.isDone) { return p.time; } else { return -1; }
        //    });
        //}

        //private System.Collections.IEnumerator UnityPingCoroutine(UnityEngine.Ping ping, float timeoutInMs) {
        //    float nrOfChecks = 30;
        //    var wait = new WaitForSeconds(timeoutInMs / nrOfChecks / 1000f);
        //    for (int i = 0; i < nrOfChecks && !ping.isDone; i++) { yield return wait; }
        //}
    }

}
