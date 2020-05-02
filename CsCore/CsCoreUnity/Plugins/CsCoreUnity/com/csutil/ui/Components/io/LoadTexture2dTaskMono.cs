using System;
using System.Collections;
using System.Threading.Tasks;
using com.csutil.http;
using UnityEngine;
using UnityEngine.Networking;

namespace com.csutil.ui.Components {

    public class LoadTexture2dTaskMono : MonoBehaviour {

        private Response<Texture2D> response;

        public Task<Texture2D> LoadFromUrl(string imageUrl, int maxNrOfRetries = 2) {
            if (imageUrl.IsNullOrEmpty()) { throw new ArgumentNullException("The passed imageUrl cant be null"); }
            response = new Response<Texture2D>();
            if (!gameObject.activeInHierarchy) { throw new Exception("The images GameObject is not active, cant load url"); }
            return TaskV2.TryWithExponentialBackoff(() => StartLoading(imageUrl), maxNrOfRetries: maxNrOfRetries, initialExponent: 10);
        }

        private Task<Texture2D> StartLoading(string imageUrl) {
            var runningTask = this.StartCoroutineAsTask(LoadFromUrlCoroutine(imageUrl, response), () => {
                var result = response.getResult();
                response = null; // Set back to null to indicate the task is done
                return result;
            });
            return UnityRestRequest.WrapWithResponseErrorHandling(response, runningTask);
        }

        private IEnumerator LoadFromUrlCoroutine(string imageUrl, Response<Texture2D> response) {
            yield return UnityWebRequestTexture.GetTexture(imageUrl).SendWebRequestV2(response);
        }

        private void OnDestroy() {
            response?.request?.Abort();
            response = null;
        }

    }

}