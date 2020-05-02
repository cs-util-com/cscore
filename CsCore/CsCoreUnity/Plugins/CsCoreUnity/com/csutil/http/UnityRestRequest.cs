using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;
        private Headers requestHeaders;

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
            var runningResTask = WebRequestRunner.GetInstance(this).StartCoroutineAsTask(prepareRequest(resp), () => resp.getResult());
            return await WrapWithResponseErrorHandling(resp, runningResTask);
        }

        public static async Task<T> WrapWithResponseErrorHandling<T>(Response<T> response, Task<T> runningResTask) {
            TaskCompletionSource<T> onErrorTask = new TaskCompletionSource<T>();
            response.onError = (_, e) => { onErrorTask.SetException(e); };
            return await Task.WhenAny<T>(runningResTask, onErrorTask.Task).Unwrap();
        }

        private IEnumerator prepareRequest<T>(Response<T> response) {
            yield return new WaitForSeconds(0.05f); // wait 5ms so that headers etc can be set
            request.SetRequestHeaders(requestHeaders);
            yield return request.SendWebRequestV2(response);
        }

        public RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType) {
            request.uploadHandler = new UploadHandlerRaw(encoding.GetBytes(textContent));
            request.SetRequestHeader("content-type", mediaType);
            return this;
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            this.requestHeaders = requestHeaders;
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