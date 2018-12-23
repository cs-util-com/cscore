using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace com.csutil.http {

    public class UnityRestRequest : RestRequest {

        private UnityWebRequest request;

        public UnityRestRequest(UnityWebRequest request) { this.request = request; }

        public Task<T> GetResult<T>(Action<T> onResult = null) {
            var r = new Response<T>();
            IoC.inject.Get<WebRequestRunner>(this).StartCoroutine(request.SendWebRequestV2(r));
            return new Task<T>(() => {
                while (!r.request.isDone) {
                    Log.d("Waiting..");
                    Thread.Sleep(5);
                }
                return r.getResult();
            });
        }

    }

}