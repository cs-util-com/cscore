using System;
using System.Threading.Tasks;

namespace com.csutil.http.apis.iplookup {
    public static class IpApiCom {

        public static Task<Response> GetResponse() {
            return new Uri("http://ip-api.com/json/").SendGET().GetResult<Response>();
        }

        public class Response { // generated via http://json2csharp.com :
            public string @as { get; set; }
            public string city { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string isp { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string org { get; set; }
            public string query { get; set; }
            public string region { get; set; }
            public string regionName { get; set; }
            public string status { get; set; }
            public string timezone { get; set; }
            public string zip { get; set; }
        }

    }
}