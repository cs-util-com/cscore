using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;
        private Headers requestHeaders;

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>() {
            var resp = new Response<T>();
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
            if (request.isModifiable) { throw new Exception("Request was not send yet, can't get result headers"); }
            while (!request.isDone && !request.isHttpError && !request.isNetworkError) { await TaskV2.Delay(5); }
            return request.GetResponseHeadersV2();
        }

    }

}