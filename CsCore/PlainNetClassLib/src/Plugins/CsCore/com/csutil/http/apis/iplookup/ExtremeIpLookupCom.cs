using System;
using System.Threading.Tasks;

namespace com.csutil.http.apis.iplookup {
    public static class ExtremeIpLookupCom {

        public static Task<Response> GetResponse() {
            return new Uri("https://extreme-ip-lookup.com/json/").SendGET().GetResult<Response>();
        }

        public class Response { // generated via http://json2csharp.com :
            public string businessName { get; set; }
            public string businessWebsite { get; set; }
            public string city { get; set; }
            public string continent { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string ipName { get; set; }
            public string ipType { get; set; }
            public string isp { get; set; }
            public string lat { get; set; }
            public string lon { get; set; }
            public string org { get; set; }
            public string query { get; set; }
            public string region { get; set; }
            public string status { get; set; }
        }

    }
}