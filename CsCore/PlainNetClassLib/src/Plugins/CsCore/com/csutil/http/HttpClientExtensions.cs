using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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

        [Obsolete("Does not work as expected, seems to laod stream fully into memory")]
        public static async Task<Stream> ReadAsStreamAsyncV2(this HttpContent c) {
            // Ensure the content is a StreamContent so that making it seakable does not cause to fully load it into memory:
            if (!(c is StreamContent)) { c = new StreamContent(await c.ReadAsStreamAsync()); }
            await c.LoadIntoBufferAsync(); // Ensures the returned stream is seakable
            return await c.ReadAsStreamAsync(); // Return the seakable stream
        }

    }
}