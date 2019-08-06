using System;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace com.csutil.http {

    public class RestFactory {

        public static RestFactory instance { get { return IoC.inject.GetOrAddSingleton<RestFactory>(new object()); } }

        public virtual RestRequest SendRequest(Uri uri, HttpMethod method) {
            return new UriRestRequest(uri).Send(method);
        }

        public virtual Task<long> GetCurrentPing(string ipOrUrl = "8.8.8.8", int timeoutInMs = 500) {
            Task<PingReply> pingTask = new Ping().SendPingAsync(ipOrUrl, timeoutInMs);
            AssertV2.IsNotNull(pingTask, "ping");
            return pingTask.ContinueWith(finishedPingTask => {
                var pingReply = finishedPingTask.Result;
                AssertV2.IsNotNull(pingReply, "result");
                return pingReply.RoundtripTime;
            }); // return ping in MS
        }

        public Task<bool> CheckInetConnection(Action hasInet, Action noInet = null, string ip = "8.8.8.8", int timeoutMs = 500) {
            return GetCurrentPing(ip, timeoutMs).ContinueWith(pingTask => {
                if (pingTask.Result > 0) { hasInet.InvokeIfNotNull(); return true; } else { noInet.InvokeIfNotNull(); return false; }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

    }

}