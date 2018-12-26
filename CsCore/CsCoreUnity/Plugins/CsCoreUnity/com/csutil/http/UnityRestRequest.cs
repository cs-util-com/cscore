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
            var mono = IoC.inject.GetOrAddComponentSingleton<WebRequestRunner>(this);
            var tcs = new TaskCompletionSource<T>();
            mono.StartCoroutine(prepareRequest(response, () => {
                try { tcs.TrySetResult(response.getResult()); }
                catch (Exception e) { tcs.TrySetException(e); }
            }));
            return tcs.Task;
        }

        private IEnumerator prepareRequest<T>(Response<T> response, Action onRequestCoroutineFinished) {
            yield return new WaitForSeconds(0.05f); // wait 5ms so that headers etc can be set
            request.SetRequestHeaders(requestHeaders);
            yield return request.SendWebRequestV2(response);
            onRequestCoroutineFinished();
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) {
            this.requestHeaders = requestHeaders;
            return this;
        }

        private class CoroutineScheduler : TaskScheduler {
            private Coroutine c;

            public CoroutineScheduler(Coroutine c) {
                this.c = c;
            }

            protected override IEnumerable<Task> GetScheduledTasks() {
                Log.MethodEntered();
                return null;
            }

            protected override void QueueTask(Task task) {
                Log.MethodEntered("task=" + task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
                Log.MethodEntered("task=" + task, "taskWasPreviouslyQueued=" + taskWasPreviouslyQueued);
                throw new NotImplementedException();
            }
        }
    }

}