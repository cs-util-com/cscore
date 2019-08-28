using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using com.csutil.http;
using Xunit;

namespace com.csutil.tests.http {

    public class RestTests {

        public RestTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            RestRequest request = new Uri("https://httpbin.org/get").SendGET();
            // Send the request and parse the response into the HttpBinGetResp class:
            HttpBinGetResp response = await request.GetResult<HttpBinGetResp>();
            Log.d("Your external IP is " + response.origin);
        }

        public class HttpBinGetResp { // The https://httpbin.org/get json as a class
            public string origin { get; set; }
            public Dictionary<string, object> headers { get; set; }
        }

        [Fact]
        public async Task TestSendGET1() {
            var x = await new Uri("https://httpbin.org/get").SendGET().GetResult<HttpBinGetResp>();
            Assert.NotNull(x);
            Log.d("Your external IP is " + x.origin);
            Assert.NotNull(x.origin);
        }

        [Fact]
        public async Task TestSendGET2() {
            RestRequest request = new Uri("https://httpbin.org/get").SendGET();
            await ValidateResponse(request);
        }

        [Fact]
        public async Task TestRestFactory1() {
            RestRequest request = RestFactory.instance.SendRequest(new Uri("https://httpbin.org/get"), HttpMethod.Get);
            await ValidateResponse(request);
            Log.d("Will now call await request.GetResultHeaders..");
            var resultHeaders = await request.GetResultHeaders();
            Assert.NotEmpty(resultHeaders);
        }

        private static async Task ValidateResponse(RestRequest request) {
            var includedRequestHeaders = new Dictionary<string, string>();
            includedRequestHeaders.Add("Aaa", "aaa 1");
            includedRequestHeaders.Add("Bbb", "bbb 2");
            request.WithRequestHeaders(new Headers(includedRequestHeaders));
            var response = await request.GetResult<HttpBinGetResp>();
            Assert.NotNull(response);
            Log.d("Your external IP is " + response.origin);
            Assert.NotNull(response.origin);

            Log.d("response.headers contain the following elements:");
            foreach (var h in response.headers) { Log.d(" > " + h.Key + " (with value " + h.Value + ")"); }

            foreach (var sentHeader in includedRequestHeaders) {
                Log.d("Now looking for " + sentHeader.Key + " (with value " + sentHeader.Value + ")");
                Assert.Equal(sentHeader.Value, response.headers.First(x => x.Key.Equals(sentHeader.Key)).Value);
            }
        }

        [Fact]
        public async Task TestGetCurrentPing() {
            var pingInMs = await RestFactory.instance.GetCurrentPing();
            Assert.NotEqual(-1, pingInMs);
            Assert.True(0 <= pingInMs && pingInMs < 500, "pingInMs=" + pingInMs);

            var hasInet = false;
            var hasNoInet = false;
            await RestFactory.instance.CheckInetConnection(() => { hasInet = true; }, () => { hasNoInet = true; });
            Assert.True(hasInet || hasNoInet); // Any of the 2 callbacks was triggered
        }

    }

}