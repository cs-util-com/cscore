using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.csutil.http {

    public interface RestRequest : IDisposable {

        Uri uri { get; }
        string httpMethod { get; }

        /// <summary> Adds a text content to the request (typically in form of UTF8 encoded json) </summary>
        /// <param name="textContent"> e.g. a json string </param>
        /// <param name="encoding"> e.g. Encoding.UTF8 </param>
        /// <param name="mediaType"> e.g. "application/json" </param>
        RestRequest WithTextContent(string textContent, Encoding encoding, string mediaType);

        RestRequest WithFormContent(Dictionary<string, object> formData);

        RestRequest WithStreamContent(Stream octetStream);
        
        RestRequest WithRequestHeaders(Headers requestHeaders);

        RestRequest WithTimeoutInMs(int timeoutInMs);

        Task<T> GetResult<T>();

        Task<Headers> GetResultHeaders();

        /// <summary> A value between 0 and 100 </summary>
        Action<float> onProgress { get; set; }

        /// <summary> Can be awaited to know when the request was started/send </summary>
        Task RequestStartedTask { get; }
        
        /// <summary> The <see cref="cancellationTokenSource"/> to cancel the request </summary>
        CancellationTokenSource CancellationTokenSource { get; }
        
    }

}