using System;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil.http {
    public interface RestRequest {

        /// <summary> Adds a text content to the request (typically in form of UTF8 encoded json) </summary>
        /// <param name="textContent"> e.g. a json string </param>
        /// <param name="encoding"> e.g. Encoding.UTF8 </param>
        /// <param name="mediaType"> e.g. "application/json" </param>
        RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType);

        RestRequest WithRequestHeaders(Headers requestHeaders);

        Task<T> GetResult<T>();

        Task<Headers> GetResultHeaders();

    }
}