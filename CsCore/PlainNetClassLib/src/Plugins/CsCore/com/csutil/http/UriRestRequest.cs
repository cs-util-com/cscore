using System;
using System.Net.Http;
using System.Threading.Tasks;
using com.csutil.http;

namespace com.csutil.http {
    internal class UriRestRequest : RestRequest {
        private Uri uri;
        public Action<UriRestRequest, HttpResponseMessage> handleResult = defaultHandler;
        public IJsonReader jsonReader = JsonReader.NewReader();
        private Task sendTask;

        public UriRestRequest(Uri uri) { this.uri = uri; }

        public Task<T> getResult<T>(Action<T> successCallback) {
            Func<T> getResult = null;
            handleResult = (self, resp) => {
                using (var t = resp.Content.ReadAsStringAsync()) {
                    var parsedResult = parseResultStringInto<T>(t.Result);
                    getResult = () => { return parsedResult; };
                    successCallback?.Invoke(parsedResult);
                }
            };
            return sendTask.ContinueWith<T>((_) => { return getResult(); });
        }

        private T parseResultStringInto<T>(string result) { return jsonReader.Read<T>(result); }

        public RestRequest send(HttpMethod method) {
            sendTask = Task.Run(() => {
                using (var c = new HttpClient()) {
                    using (var req = c.SendAsync(new HttpRequestMessage(method, uri))) {
                        handleResult.InvokeIfNotNull(this, req.Result); // calling resp.Result blocks the thread
                    }
                }
            });
            return this;
        }

        private static void defaultHandler(UriRestRequest self, HttpResponseMessage result) {
            Log.d("Rest-result for " + self.uri + ": " + result);
        }

    }
}