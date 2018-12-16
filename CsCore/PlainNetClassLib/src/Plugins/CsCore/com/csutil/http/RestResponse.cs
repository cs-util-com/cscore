using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace com.csutil.http {
    public interface RestResponse {

        RestResponse send(HttpMethod method);
        Task onResult<T>(Action<T> onResult);
    }
}