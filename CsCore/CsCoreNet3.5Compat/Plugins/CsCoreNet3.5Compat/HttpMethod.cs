#if NET_2_0 || NET_2_0_SUBSET

namespace System.Net.Http {

    public class HttpMethod {

        public static HttpMethod Get { get { return new HttpMethod("GET"); } }
        public static HttpMethod Post { get { return new HttpMethod("POST"); } }
        public static HttpMethod Put { get { return new HttpMethod("PUT"); } }
        public static HttpMethod Delete { get { return new HttpMethod("DELETE"); } }
        public static HttpMethod Head { get { return new HttpMethod("HEAD"); } }
        public static HttpMethod Options { get { return new HttpMethod("OPTIONS"); } }

        private string method;
        private HttpMethod(string method) { this.method = method; }
        public override string ToString() { return method; }

    }

}

#endif