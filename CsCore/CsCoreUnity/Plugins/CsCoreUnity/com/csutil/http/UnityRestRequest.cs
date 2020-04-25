using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.datastructures;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;
        private Headers requestHeaders;

        /// <summary> A value between 0 and 100 </summary>
        public Action<float> onProgress { get; set; }

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>() {
            var resp = new Response<T>();
            resp.WithProgress(onProgress);
            return WebRequestRunner.GetInstance(this).StartCoroutineAsTask(prepareRequest(resp), () => resp.getResult());
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
            while (!request.isDone && !request.isHttpError && !request.isNetworkError) { await TaskV2.Delay(5); }
            return request.GetResponseHeadersV2();
        }

    }

}