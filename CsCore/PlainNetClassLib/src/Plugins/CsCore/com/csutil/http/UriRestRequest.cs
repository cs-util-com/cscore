using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.http {

    internal class UriRestRequest : RestRequest, IDisposable {

        public IJsonReader jsonReader = JsonReader.GetReader();
        public Action<float> onProgress { get; set; }
        public HttpCompletionOption sendAsyncCompletedAfter = HttpCompletionOption.ResponseHeadersRead;

        public Uri uri { get; }
        public string httpMethod { get; private set; }
        private Headers requestHeaders = new Headers(new Dictionary<string, string>());
        private Task<HttpResponseMessage> request;
        private HttpClient client;
        private HttpContent httpContent;

        public UriRestRequest(Uri uri) { this.uri = uri; }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            httpContent = new StringContent(textContent, encoding, mediaType);
            return this;
        }

        public RestRequest WithFormContent(Dictionary<string, object> formData) {
            MultipartFormDataContent form = httpContent as MultipartFormDataContent;
            if (form == null) { form = new MultipartFormDataContent(); }
            foreach (var formEntry in formData) {
                if (formEntry.Value is HttpContent c) {
                    if (formEntry.Key.IsEmpty()) { form.Add(c); } else { form.Add(c, formEntry.Key); }
                } else if (formEntry.Value is byte[] bytes) {
                    form.Add(new ByteArrayContent(bytes), formEntry.Key);
                } else if (formEntry.Value is string formString) {
                    form.Add(new StringContent(formString), formEntry.Key);
                } else {
                    Log.e("Did not handle form data entry of type " + formEntry.Value.GetType());
                }
            }
            httpContent = form;
            return this;
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            if (request.IsCompleted) {
                throw new AccessViolationException("Request already sent, cant set requestHeaders anymore");
            }
            this.requestHeaders = new Headers(this.requestHeaders.AddRangeViaUnion(requestHeaders));
            return this;
        }

        public async Task<T> GetResult<T>() {
            HttpResponseMessage resp = await request;
            AssertV2.IsTrue(HttpStatusCode.OK == resp.StatusCode, "response.StatusCode=" + resp.StatusCode);
            if (TypeCheck.AreEqual<T, HttpResponseMessage>()) { return (T)(object)resp; }
            if (TypeCheck.AreEqual<T, HttpStatusCode>()) { return (T)(object)resp.StatusCode; }
            if (TypeCheck.AreEqual<T, Headers>()) { return (T)(object)await GetResultHeaders(); }
            HttpContent content = resp.Content;
            if (TypeCheck.AreEqual<T, HttpContent>()) { return (T)(object)content; }
            if (TypeCheck.AreEqual<T, Stream>()) { return (T)(object)await content.ReadAsStreamAsync(); }
            if (TypeCheck.AreEqual<T, byte[]>()) { return (T)(object)await content.ReadAsByteArrayAsync(); }
            var respText = await content.ReadAsStringAsync();
            if (typeof(T) == typeof(string)) { return (T)(object)respText; }
            AssertV2.IsNotNull(respText, "respText");
            AssertV2.IsNotNull(respText.IsNullOrEmpty(), "respText.IsNullOrEmpty");
            return jsonReader.Read<T>(respText);
        }

        public async Task<Headers> GetResultHeaders() {
            return new Headers(await GetHttpClientResultHeaders());
        }

        public async Task<HttpResponseHeaders> GetHttpClientResultHeaders() {
            HttpResponseMessage resp = await request;
            // Log.d("resp.StatusCode=" + resp.StatusCode);
            return resp.Headers;
        }

        public RestRequest Send(HttpMethod method) {
            httpMethod = "" + method;
            request = SendAsync(method);
            return this;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method) {
            client = new HttpClient();
            await TaskV2.Delay(5); // Wait so that the created RestRequest can be modified before its sent
            httpMethod = "" + method;
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
            httpContent?.Dispose();
            request?.Dispose();
            client?.Dispose();
        }

    }

}