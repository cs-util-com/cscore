using System;
using System.Threading.Tasks;

namespace com.csutil.http {
    public interface RestRequest {

        RestRequest WithRequestHeaders(Headers requestHeaders);
        Task<T> GetResult<T>();
        Task<Headers> GetResultHeaders();

    }
}