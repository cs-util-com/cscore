using System;
using System.Threading.Tasks;

namespace com.csutil.http.apis.iplookup {

    public static class IpifyOrg {

        public static Task<Response> GetResponse() {
            return new Uri("https://api.ipify.org/?format=json").SendGET().GetResult<Response>();
        }

        public class Response { public string ip; }

    }

}