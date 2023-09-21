using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.http.apis;
using com.csutil.io;
using Newtonsoft.Json.Linq;
using Xunit;

namespace com.csutil.tests.http {

    [Collection("Sequential")] // Will execute tests in here sequentially
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
            var stream = await new Uri("https://placekitten.com/" + w + "/" + h).SendGET().GetResult<Stream>();
            var image = await ImageLoader.LoadAndDispose(stream);
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task DownloadTest2() {
            var h = 110;
            var w = 60;
            var bytes = await new Uri("https://placekitten.com/" + w + "/" + h).SendGET().GetResult<byte[]>();
            var image = await ImageLoader.LoadAndDispose(new MemoryStream(bytes));
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task DownloadTest3() {
            var h = 110;
            var w = 60;
            var stream = await new Uri("https://placekitten.com/" + w + "/" + h).SendGET().GetResult<Stream>();
            var image = await ImageLoader.LoadAndDispose(stream);
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

        [Fact]
        public async Task DownloadTest4_LoadOnlyImageInfo() {
            var imgUrl = "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/logo-cscore1024x1024_2.png";
            var h = 1024;
            var w = 1024;

            var timingForFullImage = Log.MethodEntered("Load full image");
            var fullImage = await new Uri(imgUrl).SendGET().GetResult<Stream>();
            var info2 = await ImageLoader.GetImageInfoFrom(fullImage);
            fullImage.Dispose();
            Assert.Equal(h, info2.Height);
            Assert.Equal(w, info2.Width);
            Log.MethodDone(timingForFullImage);

            var timingForImageInfoOny = Log.MethodEntered("Load only first bytes");
            var stream = await new Uri(imgUrl).SendGET().GetResult<Stream>();
            var firstBytes = await ImageLoader.CopyFirstBytes(stream, bytesToCopy: 500);
            Assert.True(firstBytes.CanSeek);
            var info = await ImageLoader.GetImageInfoFrom(firstBytes);
            firstBytes.Dispose();
            Assert.Equal(w, info.Width);
            Assert.Equal(h, info.Height);
            Log.MethodDone(timingForImageInfoOny);
            stream.Dispose();

            var xTimesFaster = 3; // Loading only the image info should be at least this factor faster then loading the full image
            string e = $"{timingForImageInfoOny} was not {xTimesFaster} times faster then {timingForFullImage}!";
            Assert.True(timingForImageInfoOny.ElapsedMilliseconds * xTimesFaster < timingForFullImage.ElapsedMilliseconds, e);
        }

        [Fact]
        public async Task DownloadTest5_GetImageInfoFromFirstBytesOf() {
            var imgUrl = "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/logo-cscore1024x1024_2.png";
            var w = 1024;
            var h = 1024;
            using (var stream = await new Uri(imgUrl).SendGET().GetResult<Stream>()) {
                var info = await ImageLoader.GetImageInfoFromFirstBytesOf(stream);
                Assert.Equal(w, info.Width);
                Assert.Equal(h, info.Height);
            }
        }

        [Fact]
        public async Task DownloadTest5_GetOnlyHeaders1() {
            var url = "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/logo-cscore1024x1024_2.png";
            Headers headers = await new Uri(url).SendGET().GetResult<Headers>();
            Assert.NotEmpty(headers);
            Assert.Equal("image/png", headers.GetContentMimeType(null));
            // The image file size of 492 KB is returned as well:
            Assert.Equal("492,78 KB", ByteSizeToString.ByteSizeToReadableString(headers.GetContentLengthInBytes(0)));
            Assert.Equal("66148616DAFA743A73F9F5284F3B1D3C.png", headers.GenerateHashNameFromHeaders());
        }

        [Fact]
        public async Task DownloadTest5_GetOnlyHeaders2() {
            var url = "https://loremflickr.com/50/50";
            Headers headers = await new Uri(url).SendGET().GetResult<Headers>();
            Assert.NotEmpty(headers);
            Assert.NotNull(headers.GetContentMimeType(null));
            Assert.NotEqual(-1, headers.GetContentLengthInBytes(-1));
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

        [Fact]
        public async Task TestDateTimeV2() {
            GetDiffBetweenV1AndV2(); // Force lazy load/init of DateTimeV2 instance
            // Turn off that any diff between local and server time is accepted:
            DateTimeV2Instance().IsAcceptableDistanceToLocalTime = (_) => false;

            const int maxDiffInMs = 10000;
            // No diff between DateTime and DateTimeV2 until first server timestamp is received:
            var diffBetweenV1AndV2 = GetDiffBetweenV1AndV2();
            Assert.True(diffBetweenV1AndV2 < 100, "GetTimeDiff()=" + diffBetweenV1AndV2);

            var serverTimeReceived = false;
            EventBus.instance.Subscribe(this, DateTimeV2.SERVER_UTC_DATE, (Uri uri, DateTime serverUtcTime) => {
                serverTimeReceived = true;
                var now = DateTime.UtcNow;
                var diff = now - serverUtcTime;
                Log.d($"Server {uri} reports server time: {serverUtcTime.ToReadableStringExact()}, diff={diff.TotalMillisecondsAbs()}ms to device/system time " + now.ToReadableStringExact());
                Assert.True(diff.TotalMillisecondsAbs() < 10000, "Difference between system time and server time was " + diff);
            });

            // Trigger any REST request to get a UTC time from the used server:
            Headers headers = await new Uri("https://google.com").SendGET().GetResultHeaders();
            Assert.True(serverTimeReceived);

            string serverUtcString = headers.First(h => h.Key == "date").Value.First();
            DateTime serverUtcTime = DateTimeV2.ParseUtc(serverUtcString);
            Log.d("Server reported its UTC time to be: " + serverUtcTime);
            if (DateTimeV2Instance().diffOfLocalToServer != null) {
                var diffLocalAndOnline = DateTimeV2Instance().diffOfLocalToServer.Value.TotalMillisecondsAbs();
                Assert.True(diffLocalAndOnline < maxDiffInMs, $"diffLocalAndOnline {diffLocalAndOnline} > maxDiffInMs {maxDiffInMs}");
                Assert.NotEqual(0, diffLocalAndOnline);
            }

            // Now the server utc date should be used which will cause the diff to be larger:
            var t = GetDiffBetweenV1AndV2();
            if (t < diffBetweenV1AndV2) {
                Log.e($"GetTimeDiff()={t}ms < diffBetweenV1AndV2 ({diffBetweenV1AndV2}ms)");
            }
        }

        [Fact]
        public async Task TestDateTimeV2Consinstency() {
            for (int i = 0; i < 10; i++) {
                var uriList = new List<Uri>();
                uriList.Add(new Uri("https://google.com"));
                uriList.Add(new Uri("https://httpbin.org/get"));
                // uriList.Add(new Uri("https://wikipedia.org")); // Returns Date header in local time instead of UTC, reported at https://phabricator.wikimedia.org/T304787

                new Random().ShuffleList(uriList);

                var t1 = DateTimeV2.Now;
                var utc1 = DateTimeV2.UtcNow;
                DateTimeV2Instance().RequestUpdateOfDiffOfLocalToServer = true;
                foreach (var uri in uriList) {
                    var responseHeaders = await uri.SendHEAD().GetResultHeaders();

                    var t2 = DateTimeV2.Now;
                    var utc2 = DateTimeV2.UtcNow;

                    var maxDiffInMs = 15000; // 15 seconds offset between server time and local time is acceptable 
                    Assert.True((t2 - t1).TotalMillisecondsAbs() < maxDiffInMs, "(t2-t1)=" + (t2 - t1));
                    Assert.True((utc2 - utc1).TotalMillisecondsAbs() < maxDiffInMs, "(utc2 - utc1)=" + (utc2 - utc1));

                    var diffOfLocalToServer = DateTimeV2Instance().diffOfLocalToServer;
                    Assert.True(diffOfLocalToServer == null || diffOfLocalToServer.Value.TotalMillisecondsAbs() < maxDiffInMs, "diffOfLocalToServer=" + diffOfLocalToServer);

                    DateTimeV2Instance().RequestUpdateOfDiffOfLocalToServer = true;
                    utc1 = utc2;
                }
            }
        }

        private DateTimeV2 DateTimeV2Instance() { return IoC.inject.Get<IClock>(this) as DateTimeV2; }

        private static double GetDiffBetweenV1AndV2() { return (DateTime.UtcNow - DateTimeV2.UtcNow).TotalMillisecondsAbs(); }

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

        [Fact]
        public async Task AskStackOverflowCom1() {
            string answer = await StackOverflowCom.CheckError("How to sort a list", new List<string>() { "C#", "list" }, maxResults: 2);
            Log.d(answer);
            Assert.True(answer.Length > 600, "answer.Length=" + answer.Length);
        }

        [Fact]
        public async Task AskStackOverflowCom2() {
            string answer = await StackOverflowCom.CheckError("Sequence contains no elements", new List<string>() { "C#" }, maxResults: 2);
            Log.d(answer);
            Assert.True(answer.Length > 600, "answer.Length=" + answer.Length);
        }

        [Fact]
        public async Task TestStackOverflowCom() {
            if (!EnvironmentV2.isDebugMode) {
                Log.e("This test only works in DebugMode");
                return;
            }
            try {

                try { // Provoke an exception that will then be searched for on StackOverflow
                    List<string> list = new List<string>(); // List without entries
                    list.First(); // Will cause "Sequence contains no elements" exception
                } catch (Exception e) { await e.RethrowWithAnswers(); }

            } catch (Error exceptionWithAnswers) {
                // Check that the error contains detailed answers:
                var length = exceptionWithAnswers.Message.Length;
                Assert.True(length > 1500, "message length=" + length);
            }
        }

        [Fact]
        public async Task TestRestRequestNoSuccessError() {
            var error = (NoSuccessError)await new Uri("https://www.csutil.com/doesNotExst").SendGET().GetResult<Exception>();
            Assert.Equal(HttpStatusCode.NotFound, error.statusCode);
        }

        [Fact]
        public async Task TestUriSendMethods() {
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://httpbin.org/get").SendGET().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://httpbin.org/post").SendPOST().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://httpbin.org/put").SendPUT().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://httpbin.org/delete").SendDELETE().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://httpbin.org/patch").SendPATCH().GetResult<HttpStatusCode>());

            Assert.Equal(HttpStatusCode.OK, await new Uri("https://postman-echo.com/put").SendPUT().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://postman-echo.com/delete").SendDELETE().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://postman-echo.com/options").SendOPTIONS().GetResult<HttpStatusCode>());
            Assert.Equal(HttpStatusCode.OK, await new Uri("https://postman-echo.com/patch").SendRequest(HttpMethod.Patch).GetResult<HttpStatusCode>());

            // If a route does not exist the postman-echo returns a NotFound error code:
            Assert.Equal(HttpStatusCode.NotFound, await new Uri("https://postman-echo.com/delete").SendPUT().GetResult<HttpStatusCode>());
        }

        [Fact]
        public async Task TestUserAgent() {
            var resp = await new Uri("https://httpbin.org/user-agent").SendGET().GetResult<JObject>();
            Assert.True(resp.ContainsKey("user-agent"));
            Log.d("User agent: " + resp["user-agent"]);
        }

        [Fact]
        public async Task TestDifferentStatusCodes() {
            await TestStatusCode(HttpStatusCode.OK);
            await TestStatusCode(HttpStatusCode.Accepted);
            await TestStatusCode(HttpStatusCode.Forbidden);
            await TestStatusCode(HttpStatusCode.NotFound);
            await TestStatusCode(HttpStatusCode.NoContent);
        }

        private static async Task TestStatusCode(HttpStatusCode code) {
            Assert.Equal(code, await new Uri("https://httpbin.org/status/" + (int)code).SendGET().GetResult<HttpStatusCode>());
            Assert.Equal(code, await new Uri("https://postman-echo.com/status/" + (int)code).SendGET().GetResult<HttpStatusCode>());
        }

        [Fact]
        public async Task TestTimeouts() {
            int requestDurationInMs = 1000;
            var requestDurationInSec = requestDurationInMs / 1000;
            await TestTimeout("https://httpbin.org/delay/" + requestDurationInSec, requestDurationInMs, 500);
            await TestTimeout("https://postman-echo.com/delay/" + requestDurationInSec, requestDurationInMs, 500);
        }

        private static async Task TestTimeout(string urlOfRequestWithLongDuration, int requestDurationInMs, int cancelTimeoutInMs) {
            { // When setting a timeout for the request the request will be canceled before it completes
                Assert.True(cancelTimeoutInMs < requestDurationInMs);
                var duration = StopwatchV2.StartNewV2();
                await Assert.ThrowsAsync<TaskCanceledException>(async () => {
                    await new Uri(urlOfRequestWithLongDuration).SendGET().WithTimeoutInMs(cancelTimeoutInMs).GetResult<HttpStatusCode>();
                });
                Assert.True(duration.ElapsedMilliseconds > cancelTimeoutInMs, "duration=" + duration.ElapsedMilliseconds);
            }
            { // If no timeout is specified, the request will take the defined request duration: 
                var duration = StopwatchV2.StartNewV2();
                Assert.Equal(HttpStatusCode.OK, await new Uri(urlOfRequestWithLongDuration).SendGET().GetResult<HttpStatusCode>());
                Assert.True(duration.ElapsedMilliseconds > requestDurationInMs, "duration=" + duration.ElapsedMilliseconds);
            }
        }

        [Fact]
        public async Task TestDateTimeV2ParseUtc() {
            var utcString = "Sat, 13 Aug 2022 09:43:50 GMT";
            var utcTime = DateTimeV2.ParseUtc(utcString);
            Assert.True(utcTime.IsUtc());
            Assert.Equal(9, utcTime.Hour);
            Assert.Equal(43, utcTime.Minute);
        }

        [Fact]
        public async Task TestCurrentUTCtime() {
            var resp = await new Uri("https://postman-echo.com/time/now").SendGET().GetResult<string>();
            var remoteNow = DateTimeV2.ParseUtc(resp);
            Assert.True(remoteNow.IsUtc());
            var now = DateTimeV2.UtcNow;
            Assert.True(now.IsUtc());
            Assert.False(DateTimeV2.Now.IsUtc());
            var delta = now - remoteNow;
            Assert.True(delta.TotalMillisecondsAbs() < 10000, $"delta={delta}, now={now}, remoteNow={remoteNow} (from string '{resp}'");
        }

        [Fact]
        public async Task TestPlaceholderImageServices() {
            var h = 100;
            var w = 50;
            await TestPlaceholderImageService($"https://placekitten.com/{w}/{h}", h, w);
            await TestPlaceholderImageService($"https://loremflickr.com/{w}/{h}", h, w);
            await TestPlaceholderImageService($"https://baconmockup.com/{w}/{h}", h, w);
            await TestPlaceholderImageService($"https://placebear.com/{w}/{h}", h, w);
            await TestPlaceholderImageService($"https://placebeard.it/{w}x{h}", h, w);
            await TestPlaceholderImageService($"https://picsum.photos/{w}/{h}", h, w);
            // await TestPlaceholderImageService($"https://www.placecage.com/{w}/{h}", h, w);
            // await TestPlaceholderImageService($"https://www.fillmurray.com/{w}/{h}", h, w);
            // await TestPlaceholderImageService($"https://www.stevensegallery.com/{w}/{h}", h, w);
        }

        private static async Task TestPlaceholderImageService(string placeholderImageServiceUrl, int h, int w) {
            using var t = Log.MethodEnteredWith(placeholderImageServiceUrl);
            var s = await new Uri(placeholderImageServiceUrl).SendGET().GetResult<Stream>();
            var stream = await s.CopyToSeekableStreamIfNeeded(true);
            Log.MethodDoneWith(t, placeholderImageServiceUrl);
            var image = await ImageLoader.LoadAndDispose(stream);
            Assert.Equal(h, image.Height);
            Assert.Equal(w, image.Width);
        }

    }

}