using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace com.csutil.http {

    public interface IRestFactory : IDisposableV2 {

        RestRequest SendRequest(Uri uri, HttpMethod method);

        Task<long> GetCurrentPing(string ipOrUrl = RestFactory.DEFAULT_PING_IP, int timeoutInMs = RestFactory.DEFAULT_PING_TIMEOUT);

        Task<bool> HasInternet(Action hasInet = null, Action noInet = null, string ip = RestFactory.DEFAULT_PING_IP, int timeoutMs = RestFactory.DEFAULT_PING_TIMEOUT);

    }

    public class RestFactory : IRestFactory {

        public static IRestFactory instance { get { return IoC.inject.GetOrAddSingleton<IRestFactory>(new object(), () => new RestFactory()); } }

        public const int DEFAULT_PING_TIMEOUT = 1500;
        public const string DEFAULT_PING_IP = "8.8.8.8";

        private HttpClient client;
        private HttpClientHandler handler;

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        public RestFactory() {
            InitFactory();
        }
        
        protected virtual void InitFactory() {
            handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            client = new HttpClient(handler);
        }

        public virtual RestRequest SendRequest(Uri uri, HttpMethod method) {
            return new UriRestRequest(uri, client, handler).Send(method);
        }

        public virtual async Task<long> GetCurrentPing(string ipOrUrl = DEFAULT_PING_IP, int timeoutInMs = DEFAULT_PING_TIMEOUT) {
            PingReply pingReply = await new Ping().SendPingAsync(ipOrUrl, timeoutInMs);
            AssertV2.IsNotNull(pingReply, "pingReply");
            if (pingReply.Status != IPStatus.Success) { throw new TimeoutException("Ping failed: " + pingReply.Status); }
            return pingReply.RoundtripTime; // return ping in MS
        }

        public async Task<bool> HasInternet(Action hasInet = null, Action noInet = null, string ip = DEFAULT_PING_IP, int timeoutMs = DEFAULT_PING_TIMEOUT) {
            var ping = await GetCurrentPing(ip, timeoutMs);
            if (ping >= 0) {
                hasInet.InvokeIfNotNull();
                return true;
            } else {
                noInet.InvokeIfNotNull();
                return false;
            }
        }

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            client?.Dispose();
            handler?.Dispose();
            if (IoC.inject.Get<IRestFactory>(this) == this) { IoC.inject.RemoveAllInjectorsFor<IRestFactory>(); }
            IsDisposed = DisposeState.Disposed;
        }

    }

}