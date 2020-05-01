using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.http {

    internal class UriRestRequest : RestRequest, IDisposable {

        public IJsonReader jsonReader = JsonReader.GetReader();
        public Action<float> onProgress { get; set; }
        public HttpCompletionOption sendAsyncCompletedAfter = HttpCompletionOption.ResponseHeadersRead;

        private Uri uri;
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

        public async Task<T> GetResult<T>() {
            var c = (await request).Content;
            if (TypeCheck.AreEqual<T, Stream>()) { return (T)(object)await c.ReadAsStreamAsync(); }
            if (TypeCheck.AreEqual<T, byte[]>()) { return (T)(object)await c.ReadAsByteArrayAsync(); }
            var respText = await c.ReadAsStringAsync();
            if (typeof(T) == typeof(string)) { return (T)(object)respText; }
            return jsonReader.Read<T>(respText);
        }

        public async Task<Headers> GetResultHeaders() { return new Headers(await GetHttpClientResultHeaders()); }

        public async Task<HttpResponseHeaders> GetHttpClientResultHeaders() { return (await request).Headers; }

        public RestRequest Send(HttpMethod method) { request = SendAsync(method); return this; }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method) {
            client = new HttpClient();
            await TaskV2.Delay(5); // Wait so that the created RestRequest can be modified before its sent
            client.AddRequestHeaders(requestHeaders);
            var message = new HttpRequestMessage(method, uri);
            if (httpContent != null) { message.Content = httpContent; }
            request = client.SendAsync(message, sendAsyncCompletedAfter);
            var result = await request;
            var serverUtcDate = result.Headers.Date;
            if (serverUtcDate != null) { EventBus.instance.Publish(DateTimeV2.SERVER_UTC_DATE, uri, serverUtcDate.Value.DateTime); }
            return result;
        }

        public void Dispose() {
            request?.Dispose();
            client?.Dispose();
        }

    }

}