using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.io;
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
            public string url { get; set; }
            public string origin { get; set; }
            public Dictionary<string, object> args { get; set; }
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
            var image = await ImageLoader.LoadAndDispose(stream);
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task DownloadTest2() {
            var h = 110;
            var w = 60;
            var bytes = await new Uri("https://picsum.photos/" + w + "/" + h).SendGET().GetResult<byte[]>();
            var image = await ImageLoader.LoadAndDispose(new MemoryStream(bytes));
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task TestSendGET2() {
            RestRequest request = new Uri("https://httpbin.org/get").SendGET();
            await ValidateResponse(request);
        }

        [Fact]
        public async Task TestRestFactory1() {

            var serverTimeReceived = false;
            EventBus.instance.Subscribe(this, DateTimeV2.SERVER_UTC_DATE, (Uri uri, DateTime serverUtcTime) => {
                serverTimeReceived = true;
                var diff = DateTimeOffset.UtcNow - serverUtcTime;
                Log.d($"Server {uri} reports server time: {serverUtcTime}, diff={diff.Milliseconds}");
                Assert.True(Math.Abs(diff.Milliseconds) < 10000, "Difference between system time and server time was " + diff.Milliseconds);
            });

            RestRequest request = RestFactory.instance.SendRequest(new Uri("https://httpbin.org/get"), HttpMethod.Get);
            await ValidateResponse(request);
            Log.d("Will now call await request.GetResultHeaders..");
            var resultHeaders = await request.GetResultHeaders();
            Assert.NotEmpty(resultHeaders);
            Assert.True(serverTimeReceived);
        }

        [Fact]
        public async Task TestDateTimeV2() {
            const int maxDiffInMs = 50;
            Assert.True(GetDiffBetweenV1AndV2() < maxDiffInMs, "GetTimeDiff()=" + GetDiffBetweenV1AndV2());

            // Trigger any REST request to get a UTC time from the used server:
            Headers headers = await RestFactory.instance.SendRequest(new Uri("https://httpbin.org/get"), HttpMethod.Get).GetResultHeaders();
            string serverUtcString = headers.First(h => h.Key == "date").Value.First();
            DateTime serverUtcTime = DateTimeV2.ParseUtc(serverUtcString);
            Log.d("Server reported its UTC time to be: " + serverUtcTime);
            var diffBetweenLocalAndOnline = IoC.inject.Get<DateTimeV2>(this).diffOfLocalToServer.Value;
            Assert.True(Math.Abs(diffBetweenLocalAndOnline.Milliseconds) > maxDiffInMs);
            Log.d("Current DateTimeV2.UtcNow: " + DateTimeV2.UtcNow);
            await TaskV2.Delay(1000);
            Log.d("Corrected local time: " + DateTimeV2.UtcNow);

            // Now the server utc date should be used which will cause the diff to be larger:
            Assert.True(GetDiffBetweenV1AndV2() > maxDiffInMs, "GetTimeDiff()=" + GetDiffBetweenV1AndV2());
        }

        private static int GetDiffBetweenV1AndV2() { return Math.Abs((DateTime.UtcNow - DateTimeV2.UtcNow).Milliseconds); }

        private static async Task ValidateResponse(RestRequest request) {
            var includedRequestHeaders = new Dictionary<string, string>();
            includedRequestHeaders.Add("Aaa", "aaa 1");
            includedRequestHeaders.Add("Bbb", "bbb 2");
            request.WithRequestHeaders(new Headers(includedRequestHeaders));
            var response = await request.GetResult<HttpBinGetResp>();
            Assert.NotNull(response);
            Log.d("Your external IP is " + response.origin);
            Assert.NotNull(response.origin);

            Assert.Equal(HttpStatusCode.OK, await request.GetResult<HttpStatusCode>());
            Assert.NotEmpty(await request.GetResultHeaders());
            Assert.NotEmpty(await request.GetResult<Headers>());

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
            Assert.True(0 <= pingInMs && pingInMs < 1000, "pingInMs=" + pingInMs);

            var hasInet = false;
            var hasNoInet = false;
            await RestFactory.instance.HasInternet(() => { hasInet = true; }, () => { hasNoInet = true; });
            Assert.True(hasInet || hasNoInet); // Any of the 2 callbacks was triggered
        }

    }

    public class HasInternetTests : IHasInternetListener {

        private bool hasInet;

        [Fact]
        public async Task TestInternetStateListenerOnce() {
            InternetStateManager.AddListener(this);
            Assert.True(await RestFactory.instance.HasInternet());
            Assert.True(await InternetStateManager.Instance(this).HasInetAsync);
            Assert.True(InternetStateManager.Instance(this).HasInet);
            Assert.True(hasInet);
            InternetStateManager.RemoveListener(this);
        }

        [Fact]
        public async Task TestInternetStateListener20Times() {
            Assert.False(InternetStateManager.Instance(this).HasInet);
            for (int i = 0; i < 20; i++) {
                await TestInternetStateListenerOnce();
            }
        }

        Task IHasInternetListener.OnHasInternet(bool hasInet) {
            this.hasInet = hasInet;
            Assert.True(hasInet);
            return Task.FromResult(true);
        }

        [Fact]
        public void TestEscapeAndUnescapeStrings() {
            string s = "'abc'";
            var escapedUriString = Uri.EscapeDataString(s);
            Assert.Equal("%27abc%27", escapedUriString);
            var unescapedDataString = Uri.UnescapeDataString(escapedUriString);
            Assert.Equal(s, unescapedDataString);

            // HTML encoded strings can have a different format that can be decoded too:
            var unescapedHtmlString = WebUtility.HtmlDecode("&#39;abc&#39;");
            Assert.Equal(s, unescapedHtmlString);
        }

    }

}