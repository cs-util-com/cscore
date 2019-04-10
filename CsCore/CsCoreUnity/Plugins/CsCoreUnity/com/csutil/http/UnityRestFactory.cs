using System;
using System.Collections.Generic;
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
            throw new NotImplementedException("Not yet implemented for http method " + method);
        }

        public override Task<long> GetCurrentPing(string domainToPing = "8.8.8.8", int timeoutInMs = 500) {
            var pings = new List<Task<long>>();
            try { pings.Add(PingViaUnity(domainToPing, timeoutInMs)); } catch (Exception e) { Log.w("" + e); }
            try { pings.Add(base.GetCurrentPing(domainToPing, timeoutInMs)); } catch (Exception e) { Log.w("" + e); }
            return Task.Factory.ContinueWhenAny(pings.ToArray(), (quickerPing) => { return quickerPing.Result; });
        }

        private Task<long> PingViaUnity(string domainToPing, int timeoutInMs) {
            var p = new UnityEngine.Ping(domainToPing);
            return MainThread.instance.StartCoroutineAsTask<long>(UnityPingCoroutine(p, timeoutInMs), () => {
                if (p.isDone) { return p.time; } else { return -1; }
            });
        }

        private System.Collections.IEnumerator UnityPingCoroutine(Ping ping, float timeoutInMs) {
            float nrOfChecks = 30;
            var wait = new WaitForSeconds(timeoutInMs / nrOfChecks / 1000f);
            for (int i = 0; i < nrOfChecks && !ping.isDone; i++) { yield return wait; }
        }
    }

}
