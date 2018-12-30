using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using com.csutil.http;

namespace com.csutil.http {
    internal class UriRestRequest : RestRequest {
        private Uri uri;
        public Action<UriRestRequest, HttpResponseMessage> handleResult;
        public IJsonReader jsonReader = JsonReader.GetReader();
        private Task sendTask;
        private Headers requestHeaders;

        public UriRestRequest(Uri uri) { this.uri = uri; }

        public Task<T> GetResult<T>(Action<T> successCallback) {
            Task<string> readResultTask = null;
            Func<T> getResult = null;
            handleResult = (self, resp) => {
                using (readResultTask = resp.Content.ReadAsStringAsync()) {
                    var parsedResult = ParseResultStringInto<T>(readResultTask.Result);
                    getResult = () => { return parsedResult; };
                    successCallback.InvokeIfNotNull(parsedResult);
                }
            };
            return sendTask.ContinueWith<T>((_) => {
                readResultTask.Wait();
                return getResult();
            });
        }

        private T ParseResultStringInto<T>(string result) { return jsonReader.Read<T>(result); }

        public RestRequest Send(HttpMethod method) {
            sendTask = new Task(() => {
                Thread.Sleep(5); // wait 5ms so that the created RestRequest can be modified before its sent
                using (var c = new HttpClient()) {
                    AddRequestHeaders(c, requestHeaders);
                    using (var asyncRestRequest = c.SendAsync(new HttpRequestMessage(method, uri))) {
                        asyncRestRequest.Wait(); //helps so that other thread can set handleResult in time
                        handleResult.InvokeIfNotNull(this, asyncRestRequest.Result); // calling resp.Result blocks the thread
                    }
                }
            });
            sendTask.Start();
            return this;
        }

        private static bool AddRequestHeaders(HttpClient self, Headers requestHeadersToAdd) {
            if (requestHeadersToAdd.IsNullOrEmpty()) { return false; }
            bool r = true;
            foreach (var h in requestHeadersToAdd) {
                if (!self.DefaultRequestHeaders.TryAddWithoutValidation(h.Key, h.Value)) {
                    Log.e("Could not add header to request: " + h);
                    r = false;
                }
            }
            return r;
        }

        public RestRequest WithRequestHeaders(Headers requestHeaders) { this.requestHeaders = requestHeaders; return this; }

    }
}