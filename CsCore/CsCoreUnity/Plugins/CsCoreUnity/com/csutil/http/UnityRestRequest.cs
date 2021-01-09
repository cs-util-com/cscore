using com.csutil.http.cookies;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        public Uri uri => request.uri;
        private UnityWebRequest request;
        private Headers requestHeaders = new Headers(new Dictionary<string, IEnumerable<string>>());
        private WWWForm form;

        public string httpMethod => request.method;

        /// <summary> A value between 0 and 100 </summary>
        public Action<float> onProgress { get; set; }

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public async Task<T> GetResult<T>() {
            if (!request.isModifiable) { // Request was already sent
                await WaitForRequestToFinish();
                return request.GetResult<T>();
            }
            return await SendRequest(new Response<T>());
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
            yield return new WaitForSeconds(0.05f); // wait 5ms so that headers etc can be set
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> h = requestHeaders;
            if (form != null) {
                h = requestHeaders.AddRangeViaUnion(form.headers.Map(ToHeader));
                request.uploadHandler = new UploadHandlerRaw(form.data);
            }
            request.SetRequestHeaders(h);
            yield return request.SendWebRequestV2(response);
        }

        private KeyValuePair<string, IEnumerable<string>> ToHeader(KeyValuePair<string, string> header) {
            return new KeyValuePair<string, IEnumerable<string>>(header.Key, new List<string>() { header.Value });
        }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            request.uploadHandler = new UploadHandlerRaw(encoding.GetBytes(textContent));
            request.SetRequestHeader("content-type", mediaType);
            return this;
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            if (!request.isModifiable) {
                throw new AccessViolationException("Request already sent, cant set requestHeaders anymore");
            }
            this.requestHeaders = new Headers(this.requestHeaders.AddRangeViaUnion(requestHeaders));
            return this;
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

        public async Task<Headers> GetResultHeaders() {
            if (request.isModifiable) { return await GetResult<Headers>(); }
            await WaitForRequestToFinish();
            return request.GetResponseHeadersV2();
        }

        private async Task WaitForRequestToFinish() {
            while (!request.isDone && !request.isHttpError && !request.isNetworkError) { await TaskV2.Delay(10); }
        }

    }

}