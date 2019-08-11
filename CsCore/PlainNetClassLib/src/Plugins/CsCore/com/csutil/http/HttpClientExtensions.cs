using System.Net.Http;
using com.csutil.http;

namespace com.csutil {
    public static class HttpClientExtensions {

        public static bool AddRequestHeaders(this HttpClient self, Headers requestHeadersToAdd) {
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

    }
}