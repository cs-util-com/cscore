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

        public Task<long> GetCurrentPing(string domainToPing = "8.8.8.8", int timeoutInMs = 500) {
            return new Ping().SendPingAsync(domainToPing, timeoutInMs).ContinueWith((a) => {
                var pingResponse = a.Result;
                if (pingResponse != null && pingResponse.Status == IPStatus.Success) {
                    return pingResponse.RoundtripTime; // return ping in MS
                } else {
                    return -1; // No internet available
                }
            });
        }

    }

}