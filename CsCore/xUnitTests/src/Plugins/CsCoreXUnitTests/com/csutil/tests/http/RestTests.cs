using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using com.csutil.http;
using StbImageLib;
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
        public async Task DownloadTest1() {
            var h = 100;
            var w = 50;
            var stream = await new Uri("https://picsum.photos/" + w + "/" + h).SendGET().GetResult<Stream>();
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            stream.Dispose();
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task DownloadTest2() {
            var h = 110;
            var w = 60;
            var bytes = await new Uri("https://picsum.photos/" + w + "/" + h).SendGET().GetResult<byte[]>();
            using (var stream = new MemoryStream(bytes)) {
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
                Assert.Equal(h, image.Height);
                Assert.Equal(w, image.Width);
            }
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
            await RestFactory.instance.HasInternet(() => { hasInet = true; }, () => { hasNoInet = true; });
            Assert.True(hasInet || hasNoInet); // Any of the 2 callbacks was triggered
        }


    }

    public class HasInternetTests : IHasInternetListener {

        private bool hasInet;

        [Fact]
        public async Task TestInternetStateListener() {
            await InternetStateManager.AddListener(this);
            Assert.False(InternetStateManager.Get(this).hasInet);
            Assert.False(hasInet);
            await TaskV2.Delay(2500);
            Assert.True(hasInet);
        }

        Task IHasInternetListener.OnHasInternet(bool hasInet) { this.hasInet = hasInet; return Task.FromResult(true); }

    }

}