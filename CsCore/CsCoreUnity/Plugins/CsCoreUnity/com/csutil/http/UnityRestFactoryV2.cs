using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.http {

    public class UnityRestFactoryV2 : RestFactory {

        public override async Task<long> GetCurrentPing(string ipOrUrl = DEFAULT_PING_IP, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (ApplicationV2.platform.IsAnyOf(RuntimePlatform.WebGLPlayer)) {
                return await GetCurrentPingViaHeadRequest(ipOrUrl, timeoutInMs);
            }
            var ping = await GetCurrentPingViaUnityPing(ipOrUrl, timeoutInMs);
            if (ping > 0) { return ping; }
            // If Unity.Ping did not work, eg because a URL was used instead of an IP fallback to default ping approach:
            return await base.GetCurrentPing(ipOrUrl, timeoutInMs);
        }

        public static async Task<long> GetCurrentPingViaHeadRequest(string ipOrUrl, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            if (!ipOrUrl.StartsWith("http")) { ipOrUrl = "https://" + ipOrUrl; }
            Stopwatch timer = Stopwatch.StartNew();
            var result = await new Uri(ipOrUrl).SendHEAD().GetResult<HttpStatusCode>().WithTimeout(timeoutInMs);
            AssertV2.AreEqual(HttpStatusCode.OK, result);
            return timer.ElapsedMilliseconds;
        }

        private static async Task<long> GetCurrentPingViaUnityPing(string ip, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            var ping = new UnityEngine.Ping(ip);
            var timer = Stopwatch.StartNew();
            while (!ping.isDone && timer.ElapsedMilliseconds < timeoutInMs) { await TaskV2.Delay(10); }
            return ping.isDone ? ping.time : -1;
        }

    }

}