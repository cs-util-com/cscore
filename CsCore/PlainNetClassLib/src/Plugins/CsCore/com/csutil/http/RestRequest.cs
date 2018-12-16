using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace com.csutil.http {
    public interface RestRequest {

        RestRequest send(HttpMethod method);
        Task<T> getResult<T>(Action<T> onResult = null);
    }
}