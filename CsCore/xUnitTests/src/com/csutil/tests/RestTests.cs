using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests {
    public class RestTests : IDisposable {
        public RestTests() { // Setup before each test
        }
        public void Dispose() { // TearDown after each test
        }

        [Fact]
        public async Task Test1() {
            await new Uri("https://httpbin.org/get").sendGET().onResult<HttpBinGetResponse>((x) => {
                Log.d("Your external IP is " + x.origin);
                Assert.NotNull(x);
                Assert.NotNull(x.origin);
            });
        }

        public class HttpBinGetResponse {
            public Dictionary<string, object> args { get; set; }
            public string origin { get; set; }
            public string url { get; set; }
            public Headers headers { get; set; }
            public class Headers {
                public string Accept { get; set; }
                public string Accept_Encoding { get; set; }
                public string Accept_Language { get; set; }
                public string Connection { get; set; }
                public string Host { get; set; }
                public string Upgrade_Insecure_Requests { get; set; }
                public string User_Agent { get; set; }
            }
        }
    }
}