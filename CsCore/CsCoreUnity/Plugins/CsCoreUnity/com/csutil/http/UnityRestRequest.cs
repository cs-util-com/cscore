using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;
        private Headers requestHeaders;

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>(Action<T> onResult = null) {
            var response = new Response<T>();
            return WebRequestRunner.GetInstance(this).StartCoroutineAsTask(prepareRequest(response), () => response.getResult());
        }

        private IEnumerator prepareRequest<T>(Response<T> response) {
            yield return new WaitForSeconds(0.05f); // wait 5ms so that headers etc can be set
            request.SetRequestHeaders(requestHeaders);
            yield return request.SendWebRequestV2(response);
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            this.requestHeaders = requestHeaders;
            return this;
        }

    }

}