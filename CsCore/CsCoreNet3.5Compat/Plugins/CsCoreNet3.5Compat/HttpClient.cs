using System.Threading.Tasks;

namespace System.Net.Http {

    public class HttpClient : IDisposable {
        public HttpRequestHeaders DefaultRequestHeaders;

        public void Dispose() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage) {
            throw new NotImplementedException();
        }

        public class HttpRequestHeaders {
            public bool TryAddWithoutValidation(string key, string value) {
                throw new NotImplementedException();
            }
        }
    }

}
