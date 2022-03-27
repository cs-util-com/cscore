using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.http {

    public class UriRestRequest : RestRequest, IDisposable {

        public IJsonReader jsonReader = JsonReader.GetReader();
        public Action<float> onProgress { get; set; }
        public HttpCompletionOption sendAsyncCompletedAfter = HttpCompletionOption.ResponseHeadersRead;
        public Uri uri { get; }
        public string httpMethod { get; private set; }
        public Func<HttpClient, HttpRequestMessage, Task> OnBeforeSend;

        private Headers requestHeaders = new Headers(new Dictionary<string, string>());
        private Task<HttpResponseMessage> request;
        private HttpClient client;
        private HttpContent httpContent;

        private TaskCompletionSource<bool> waitForRequestToBeConfigured = new TaskCompletionSource<bool>();

        public UriRestRequest(Uri uri) { this.uri = uri; }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            httpContent = new StringContent(textContent, encoding, mediaType);
            return this;
        }

        public RestRequest WithFormContent(Dictionary<string, object> formData) {

            // For pure string based key value pairs send "application/x-www-form-urlencoded" content:
            if (formData.All(entry => entry.Value is string)) {
                if (httpContent == null) {
                    httpContent = new FormUrlEncodedContent(formData.ToDictionary(x => x.Key, x => (string)x.Value));
                    return this;
                } else {
                    Log.w("formData is all strings application/x-www-form-urlencoded could be used but " +
                        "other form content was already added so multipart/form-data will be used instead!");
                }
            }

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

        public RestRequest WithStreamContent(Stream octetStream) {
            httpContent = new StreamContent(octetStream);
            httpContent.Headers.Add("Content-Type", "application/octet-stream");
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
            waitForRequestToBeConfigured.TrySetResult(true);
            HttpResponseMessage resp = await request;
            if (typeof(T).IsCastableTo<Exception>() && resp.StatusCode.IsErrorStatus()) {
                return (T)(object)new NoSuccessError(resp.StatusCode, await GetResult<string>());
            }
            if (HttpStatusCode.OK != resp.StatusCode) { Log.w("response.StatusCode=" + resp.StatusCode); }
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
            try { return jsonReader.Read<T>(respText); }
            catch (JsonReaderException e) { throw new JsonReaderException("Cant parse to JSON: " + respText, e); }
        }

        public async Task<Headers> GetResultHeaders() {
            waitForRequestToBeConfigured.TrySetResult(true);
            HttpResponseMessage response = await request;
            var headers = new Headers(response.Headers);
            headers.AddRange(response.Content.Headers);
            return headers;
        }

        public RestRequest Send(HttpMethod method) {
            httpMethod = "" + method;
            request = SendAsync(method);
            return this;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpMethod method) {
            HttpClientHandler handler = new HttpClientHandler() {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            AddAllCookiesToRequest(handler);
            client = new HttpClient(handler);
            await waitForRequestToBeConfigured.Task.WithTimeout(timeoutInMs: 30000);
            httpMethod = "" + method;
            client.AddRequestHeaders(requestHeaders);
            var message = new HttpRequestMessage(method, uri);
            if (httpContent != null) { message.Content = httpContent; }
            if (OnBeforeSend != null) { await OnBeforeSend(client, message); }
            var result = await client.SendAsync(message, sendAsyncCompletedAfter);

            var cookieJar = IoC.inject.Get<cookies.CookieJar>(this, false);
            if (cookieJar != null) {
                cookieJar.SetCookies(handler.CookieContainer.GetCookiesForCookieJar(uri).ToArray());
            }

            var serverUtcDate = result.Headers.Date;
            if (serverUtcDate != null) { EventBus.instance.Publish(DateTimeV2.SERVER_UTC_DATE, uri, serverUtcDate.Value.DateTime); }
            return result;
        }

        private void AddAllCookiesToRequest(HttpClientHandler handler) {
            var reusableCookieContainer = IoC.inject.Get<CookieContainer>(this, false);
            if (reusableCookieContainer != null) {
                handler.CookieContainer = reusableCookieContainer;
                // Since an existing cookie container is reused it is assumed it is already filled with the correct cookies
            } else {
                var cookieJar = IoC.inject.Get<cookies.CookieJar>(this, false);
                cookieJar.LoadFromCookieJarIntoCookieContainer(uri, target: handler.CookieContainer);
            }
        }

        public void Dispose() {
            httpContent?.Dispose();
            request?.Dispose();
            client?.Dispose();
        }

    }

}