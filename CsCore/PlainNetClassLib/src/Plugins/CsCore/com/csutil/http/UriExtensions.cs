using System.Net.Http;
using com.csutil.http;

namespace System {
    public static class UriExtensions {

        public static RestRequest sendGET(this Uri self) { return new UriRestResponse(self).send(HttpMethod.Get); }

    }
}