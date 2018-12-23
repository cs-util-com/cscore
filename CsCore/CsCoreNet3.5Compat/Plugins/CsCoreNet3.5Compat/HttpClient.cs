using System.Threading.Tasks;

namespace System.Net.Http {

    public class HttpClient : IDisposable {

        public void Dispose() {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage) {
            throw new NotImplementedException();
        }

    }

}
