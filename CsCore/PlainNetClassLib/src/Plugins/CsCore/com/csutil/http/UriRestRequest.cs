using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil.http {

    public class UriRestRequest : RestRequest {

        public IJsonReader jsonReader = JsonReader.GetReader(null);
        public Action<float> onProgress { get; set; }
        public HttpCompletionOption sendAsyncCompletedAfter = HttpCompletionOption.ResponseHeadersRead;
        public Uri uri { get; }
        public string httpMethod { get; private set; }
        public Func<HttpClient, HttpRequestMessage, Task> OnBeforeSend;

        /// <summary> The timeout for the request, the default is no timeout </summary>
        public TimeSpan? Timeout { get; set; } = null;

        private Headers requestHeaders = new Headers(new Dictionary<string, string>());
        private Task<HttpResponseMessage> request;
        private HttpClient client;
        private HttpClientHandler handler;
        private HttpContent httpContent;
        private HttpRequestMessage sentRequest;

        private TaskCompletionSource<bool> waitForRequestToBeConfigured = new TaskCompletionSource<bool>();
        public Task RequestStartedTask => waitForRequestToBeConfigured.Task;

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        [Obsolete("Pass a reusable HttpClient instead via the other constructor")]
        public UriRestRequest(Uri uri) { this.uri = uri; }

        public UriRestRequest(Uri uri, HttpClient client, HttpClientHandler handler) {
            this.uri = uri;
            this.client = client;
            this.handler = handler;
        }

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
                } else if (formEntry.Value is int formInt) {
                    form.Add(new StringContent("" + formInt), formEntry.Key);
                } else if (formEntry.Value is long formLong) {
                    form.Add(new StringContent("" + formLong), formEntry.Key);
                } else if (formEntry.Value is float formFloat) {
                    form.Add(new StringContent("" + formFloat), formEntry.Key);
                } else if (formEntry.Value is double formDouble) {
                    form.Add(new StringContent("" + formDouble), formEntry.Key);
                } else if (formEntry.Value is decimal formDecimal) {
                    form.Add(new StringContent("" + formDecimal), formEntry.Key);
                } else if (formEntry.Value is bool formBool) {
                    form.Add(new StringContent("" + formBool), formEntry.Key);
                } else {
                    throw Log.e($"Did not handle form data entry of type {formEntry.Value.GetType()}: {formEntry}");
                }
            }
            httpContent = form;
            return this;
        }

        public RestRequest WithStreamContent(Stream octetStream) {
            httpContent = new StreamContent(octetStream);
            httpContent.Headers.Add("Content-Type", "application/octet-stream");
            RequestStartedTask.ContinueWith(delegate {
                if (onProgress != null) {
                    octetStream.MonitorPositionForProgress((progress) => {
                        onProgress(progress);
                    }, CancellationTokenSource).LogOnError();
                }
            });
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
                return (T)(object)await NoSuccessError.Create(this, resp.StatusCode);
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
            if (HttpStatusCode.OK != resp.StatusCode && !respText.IsNullOrEmpty()) { Log.w($"{resp.StatusCode} response:\n{respText}"); }
            if (typeof(T) == typeof(string)) {
                return (T)(object)respText;
            }
            AssertV3.IsNotNull(respText, "respText");
            AssertV3.IsNotNull(respText.IsNullOrEmpty(), "respText.IsNullOrEmpty");
            try { return jsonReader.Read<T>(respText); } catch (JsonReaderException e) { throw new JsonReaderException("Cant parse to JSON: " + respText, e); }
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

            if (handler == null) {
                handler = new HttpClientHandler() {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                };
            }
            if (client == null) {
                client = new HttpClient(handler);
            }
            AddAllCookies(handler);

            await waitForRequestToBeConfigured.Task.WithTimeout(timeoutInMs: 30000);
            httpMethod = "" + method;
            sentRequest = new HttpRequestMessage(method, uri);
            if (httpContent != null) { sentRequest.Content = httpContent; }
            sentRequest.AddRequestHeaders(requestHeaders);

            if (Timeout != null) {
                CancellationTokenSource.CancelAfter(Timeout.Value);
            }

            if (OnBeforeSend != null) { await OnBeforeSend(client, sentRequest); }
            var result = await client.SendAsync(sentRequest, sendAsyncCompletedAfter, CancellationTokenSource.Token);

            var cookieJar = IoC.inject.Get<cookies.CookieJar>(this, false);
            if (cookieJar != null) {
                cookieJar.SetCookies(handler.CookieContainer.GetCookiesForCookieJar(uri).ToArray());
            }

            try {
                var serverUtcDate = result.Headers.Date;
                if (serverUtcDate != null) {
                    EventBus.instance.Publish(DateTimeV2.SERVER_UTC_DATE, uri, serverUtcDate.Value.UtcDateTime);
                }
            } catch (Exception e) { Log.e(e); }
            return result;
        }

        private void AddAllCookies(HttpClientHandler handler) {
            var injectedCookieManager = IoC.inject.Get<CookieContainer>(this, false);
            if (injectedCookieManager != null && injectedCookieManager != handler.CookieContainer) {
                try {
                    handler.CookieContainer = injectedCookieManager;
                } catch (InvalidOperationException e) {
                    throw new InvalidOperationException("The cookie manager changes after the HttpClient was already in use, ensure the reset the "
                        + "HttpClient singleton in the RestFactory when loading a cookie manager from disc during runtime", e);
                }
            }
            var cookieJar = IoC.inject.Get<cookies.CookieJar>(this, false);
            cookieJar.LoadFromCookieJarIntoCookieContainer(uri, target: handler.CookieContainer);
        }

        public RestRequest WithTimeoutInMs(int timeoutInMs) {
            Timeout = TimeSpan.FromMilliseconds(timeoutInMs);
            return this;
        }

        public void Dispose() {
            CancellationTokenSource.Cancel();
            httpContent?.Dispose();
            sentRequest?.Dispose();
        }

    }

}