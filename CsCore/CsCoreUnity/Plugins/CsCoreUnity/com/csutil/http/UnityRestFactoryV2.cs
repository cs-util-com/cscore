using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestFactoryV2 : RestFactory {

        public override RestRequest SendRequest(Uri uri, HttpMethod method) {
            if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.WebGLPlayer)) {
                return MainThread.instance.ExecuteOnMainThread(() => {
                    if (method.ToString() == "GET") {
                        return new UnityRestRequest(UnityWebRequest.Get(uri));
                    }
                    return new UnityRestRequest(new UnityWebRequest(uri, method.ToString()));
                });
            }
            return base.SendRequest(uri, method);
        }

        public override async Task<long> GetCurrentPing(string ipOrUrl = DEFAULT_PING_IP, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.WebGLPlayer) && ApplicationV2.isPlaying) {
                return await GetCurrentPingViaHeadRequest(ipOrUrl, timeoutInMs);
            }
#if !UNITY_WEBGL
            var ping = await GetCurrentPingViaUnityPing(ipOrUrl, timeoutInMs);
            if (ping > 0) { return ping; }
            // Try fallback to head request if Unity.Ping did not work:
            ping = await GetCurrentPingViaHeadRequest(ipOrUrl, timeoutInMs);
            if (ping > 0) { return ping; }
#endif
            // If Unity.Ping did not work, eg because a URL was used instead of an IP fallback to default ping approach:
            try {
                return await base.GetCurrentPing(ipOrUrl, timeoutInMs);
            } catch (Exception e) {
                Log.e("Failed to ping: " + ipOrUrl, e);
                throw;
            }
        }

        public static async Task<long> GetCurrentPingViaHeadRequest(string ipOrUrl, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (!ipOrUrl.StartsWith("http")) { ipOrUrl = "https://" + ipOrUrl; }
            Stopwatch timer = Stopwatch.StartNew();
            var result = await new Uri(ipOrUrl).SendHEAD().GetResult<HttpStatusCode>().WithTimeout(timeoutInMs);
            AssertV2.AreEqual(HttpStatusCode.OK, result);
            return timer.ElapsedMilliseconds;
        }

#if !UNITY_WEBGL
        private static Task<long> GetCurrentPingViaUnityPing(string ip, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (!ApplicationV2.isPlaying) { return GetCurrentPingViaUnityPingInMainThread(ip, timeoutInMs); }
            return MainThread.Invoke<long>(() => GetCurrentPingViaUnityPingInMainThread(ip, timeoutInMs));
        }

        private static async Task<long> GetCurrentPingViaUnityPingInMainThread(string ip, int timeoutInMs) {
            var ping = new UnityEngine.Ping(ip);
            var timer = Stopwatch.StartNew();
            while (!ping.isDone && timer.ElapsedMilliseconds < timeoutInMs) { await TaskV2.Delay(10); }
            return ping.isDone ? ping.time : -1;
        }
#endif

    }

}