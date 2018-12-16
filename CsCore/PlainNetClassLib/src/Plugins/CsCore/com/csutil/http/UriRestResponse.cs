using System;
using System.Net.Http;
using System.Threading.Tasks;
using com.csutil.http;

namespace com.csutil.http {
    internal class UriRestResponse : RestResponse {
        private Uri uri;
        public Action<UriRestResponse, HttpResponseMessage> handleResult = defaultHandler;
        public IJsonReader jsonReader = JsonReader.NewReader();
        private Task sendTask;

        public UriRestResponse(Uri uri) { this.uri = uri; }

        public Task onResult<T>(Action<T> successCallback) {
            handleResult = (self, resp) => {
                using (var t = resp.Content.ReadAsStringAsync()) {
                    successCallback(parseResultStringInto<T>(t.Result));
                }
            };
            return sendTask;
        }

        private T parseResultStringInto<T>(string result) { return jsonReader.Read<T>(result); }

        public RestResponse send(HttpMethod method) {
            sendTask = Task.Run(() => {
                using (var c = new HttpClient()) {
                    using (var req = c.SendAsync(new HttpRequestMessage(method, uri))) {
                        handleResult.InvokeIfNotNull(this, req.Result); // calling resp.Result blocks the thread
                    }
                }
            });
            return this;
        }

        private static void defaultHandler(UriRestResponse self, HttpResponseMessage result) {
            Log.d("Rest-result for " + self.uri + ": " + result);
        }

    }
}