using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.http;

namespace com.csutil.http {

    internal class UriRestRequest : RestRequest, IDisposable {

        private Uri uri;
        public IJsonReader jsonReader = JsonReader.GetReader();
        private Headers requestHeaders;
        private Task<HttpResponseMessage> request;
        private HttpClient client;

        public UriRestRequest(Uri uri) { this.uri = uri; }
        public RestRequest WithRequestHeaders(Headers requestHeaders) { this.requestHeaders = requestHeaders; return this; }

        public async Task<T> GetResult<T>() { return jsonReader.Read<T>(await (await request).Content.ReadAsStringAsync()); }

        public async Task<Headers> GetResultHeaders() { return new Headers((await request).Headers); }

        public RestRequest Send(HttpMethod method) {
            client = new HttpClient();
            request = SendWithDelay(method);
            return this;
        }

        private async Task<HttpResponseMessage> SendWithDelay(HttpMethod method) {
            await Task.Delay(5); // wait 5ms so that the created RestRequest can be modified before its sent
            client.AddRequestHeaders(requestHeaders);
            return await client.SendAsync(new HttpRequestMessage(method, uri));
        }

        public void Dispose() {
            request?.Dispose();
            client?.Dispose();
        }

    }

}