using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
        private HttpContent httpContent;

        public UriRestRequest(Uri uri) { this.uri = uri; }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            httpContent = new StringContent(textContent, encoding, mediaType);
            return this;
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) { this.requestHeaders = requestHeaders; return this; }

        public async Task<T> GetResult<T>() { return jsonReader.Read<T>(await (await request).Content.ReadAsStringAsync()); }

        public async Task<Headers> GetResultHeaders() { return new Headers((await request).Headers); }

        public RestRequest Send(HttpMethod method) { request = SendAsync(method); return this; }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method) {
            client = new HttpClient();
            await TaskV2.Delay(5); // Wait so that the created RestRequest can be modified before its sent
            client.AddRequestHeaders(requestHeaders);
            var message = new HttpRequestMessage(method, uri);
            if (httpContent != null) { message.Content = httpContent; }
            request = client.SendAsync(message);
            return await request;
        }

        public void Dispose() {
            request?.Dispose();
            client?.Dispose();
        }

    }

}