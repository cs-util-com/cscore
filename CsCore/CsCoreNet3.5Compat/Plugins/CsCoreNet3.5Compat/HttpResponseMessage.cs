using System.Threading.Tasks;

namespace System.Net.Http {
    public class HttpResponseMessage {
        public Response Content { get; private set; }

        public class Response {
            public Task<string> ReadAsStringAsync() {
                throw new NotImplementedException();
            }
        }
    }
}
