using System;
using System.Threading.Tasks;

namespace com.csutil.http {
    public interface RestRequest {

        Task<T> GetResult<T>(Action<T> onResult = null);
    }
}