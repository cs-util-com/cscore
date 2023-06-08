using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        public Uri uri => request.uri;
        private UnityWebRequest request;
        private Headers requestHeaders = new Headers(new Dictionary<string, IEnumerable<string>>());
        private WWWForm form;
        private Stream streamToSend;

        public string httpMethod => request.method;

        /// <summary> A value between 0 and 100 </summary>
        public Action<float> onProgress { get; set; }

        private TaskCompletionSource<bool> waitForRequestToBeConfigured = new TaskCompletionSource<bool>();
        public Task RequestStartedTask => waitForRequestToBeConfigured.Task;

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

        /// <summary> The timeout for the request, the default is no timeout </summary>
        public TimeSpan? Timeout { get; set; } = null;

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>() {
            waitForRequestToBeConfigured.TrySetResult(true);
            return MainThread.instance.ExecuteOnMainThreadAsync(async () => {
                if (!request.isModifiable) { // Request was already sent
                    await WaitForRequestToFinish();
                    return request.GetResult<T>();
                }
                var response = new Response<T>();
                if (request.downloadHandler != null) {
                    response.createDownloadHandler = () => request.downloadHandler;
                }
                return await SendRequest(response);
            });
        }

        private async Task<T> SendRequest<T>(Response<T> resp) {
            resp.WithProgress(onProgress);
            return await WebRequestRunner.GetInstance(this).StartCoroutineAsTask(PrepareRequest(resp), () => resp.getResult());
        }

        public static async Task<T> WrapWithResponseErrorHandling<T>(Response<T> response, Task<T> runningResTask) {
            TaskCompletionSource<T> onErrorTask = new TaskCompletionSource<T>();
            response.onError = (_, e) => { onErrorTask.SetException(e); };
            return await Task.WhenAny<T>(runningResTask, onErrorTask.Task).Unwrap();
        }

        private IEnumerator PrepareRequest<T>(Response<T> response) {
            yield return waitForRequestToBeConfigured.Task.WithTimeout(30000).AsCoroutine();
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> h = requestHeaders;
            if (form != null) {
                h = requestHeaders.AddRangeViaUnion(form.headers.Map(ToHeader));
                request.uploadHandler = new UploadHandlerRaw(form.data);
            }
            if (streamToSend != null) {
                if (form != null) { throw new DataMisalignedException("Cant have both a form and a stream as the request content"); }
                request.uploadHandler = new UploadHandlerRaw(streamToSend.ToByteArray());
            }
            request.SetRequestHeaders(h);
            if (Timeout != null) { request.timeout = (int)Timeout.Value.TotalSeconds; }
            yield return request.SendWebRequestV2(response);
        }

        private KeyValuePair<string, IEnumerable<string>> ToHeader(KeyValuePair<string, string> header) {
            return new KeyValuePair<string, IEnumerable<string>>(header.Key, new List<string>() { header.Value });
        }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            return MainThread.instance.ExecuteOnMainThread(() => {
                request.uploadHandler = new UploadHandlerRaw(encoding.GetBytes(textContent));
                request.SetRequestHeader("content-type", mediaType);
                return this;
            });
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            return MainThread.instance.ExecuteOnMainThread(() => {
                if (!request.isModifiable) {
                    throw new AccessViolationException("Request already sent, cant set requestHeaders anymore");
                }
                this.requestHeaders = new Headers(this.requestHeaders.AddRangeViaUnion(requestHeaders));
                return this;
            });
        }

        public RestRequest WithFormContent(Dictionary<string, object> formData) {
            if (form == null) { form = new WWWForm(); }
            foreach (var formEntry in formData) {
                string key = formEntry.Key;
                object v = formEntry.Value;
                string fileName = null;
                if (v is HttpContent c) {
                    if (key.IsEmpty()) { key = c.Headers.ContentDisposition.Name; }
                    fileName = c.Headers.ContentDisposition?.FileName;
                    v = c.ReadAsStreamAsync().Result;
                }
                if (v is Stream s) { v = s.ToByteArray(); }
                if (v is byte[] bytes) {
                    if (fileName.IsNullOrEmpty()) {
                        form.AddBinaryData(key, bytes);
                    } else {
                        form.AddBinaryData(key, bytes, fileName);
                    }
                } else if (v is string formString) {
                    form.AddField(key, formString);
                } else {
                    Log.e("Did not handle form data entry of type " + v.GetType());
                }
            }
            return this;
        }

        public RestRequest WithStreamContent(Stream octetStream) {
            this.streamToSend = octetStream;
            return this;
        }

        public Task<Headers> GetResultHeaders() {
            return MainThread.instance.ExecuteOnMainThreadAsync(async () => {
                waitForRequestToBeConfigured.TrySetResult(true);
                if (request.isModifiable) { return await GetResult<Headers>(); }
                await WaitForRequestToFinish();
                return request.GetResponseHeadersV2();
            });
        }

        private async Task WaitForRequestToFinish() {
            while (!request.isDone && !request.isHttpError && !request.isNetworkError) {
                if (CancellationTokenSource.IsCancellationRequested) { request.Abort(); }
                CancellationTokenSource.Token.ThrowIfCancellationRequested();
                await TaskV2.Delay(10);
            }
        }

        public RestRequest WithTimeoutInMs(int timeoutInMs) {
            Timeout = TimeSpan.FromMilliseconds(timeoutInMs);
            return this;
        }

        public void Dispose() {
            CancellationTokenSource.Cancel();
            try { request.Abort(); } catch (Exception e) { Log.d("Could not abort request: " + e); }
        }

    }

}