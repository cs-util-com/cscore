using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.http;
using Xunit;

namespace com.csutil.tests {
    public class RestTests : IDisposable {

        public RestTests() { // Setup before each test
        }

        public void Dispose() { // TearDown after each test
        }

        [Fact]
        public async Task TestSendGET1() {
            await new Uri("https://httpbin.org/get").SendGET().GetResult<HttpBinGetResp>((x) => {
                Assert.NotNull(x);
                Log.d("Your external IP is " + x.origin);
                Assert.NotNull(x.origin);
            });
        }

        [Fact]
        public async Task TestSendGET2() {
            RestRequest request = new Uri("https://httpbin.org/get").SendGET();
            await ValidateResponse(request);
        }

        [Fact]
        public async Task TestRestFactory1() {
            RestRequest request = RestFactory.instance.SendGET(new Uri("https://httpbin.org/get"));
            await ValidateResponse(request);
        }

        private static async Task ValidateResponse(RestRequest request) {
            var response = await request.GetResult<HttpBinGetResp>();
            Assert.NotNull(response);
            Log.d("Your external IP is " + response.origin);
            Assert.NotNull(response.origin);
        }

        public class HttpBinGetResp {
            public Dictionary<string, object> args { get; set; }
            public string origin { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> headers { get; set; }
        }
    }
}