using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using Xunit;

namespace com.csutil.tests.logging {

    public class GoogleAnalyticsTests {

        // https://analytics.google.com/analytics/web/?pli=1#/report/content-event-events/a130164002w221792235p210768996/
        private const string TEST_APP_KEY = "UA-130164002-5";

        public GoogleAnalyticsTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestConvertEventToQueryParams() {
            var googleAnalytics = new GoogleAnalytics(TEST_APP_KEY, new InMemoryKeyValueStore()) {
                url = GoogleAnalytics.DEBUG_ENDPOINT // Use the debug endpoint
            };
            var e = googleAnalytics.NewEvent("cat1", "action1", "label1", value: 123);
            var queryParams = RestRequestHelper.ToUriEncodedString(e);
            var fullUrl = googleAnalytics.url + "?" + queryParams;
            var res = await new Uri(fullUrl).SendGET().GetResult<GoogleAnalyticsDebugResp>();
            Assert.True(res.hitParsingResult.First().valid, JsonWriter.AsPrettyString(res));
        }

        [Fact]
        public async Task TestSendEventToGA() {
            var googleAnalytics = new GoogleAnalytics(TEST_APP_KEY, new InMemoryKeyValueStore()) {
                url = GoogleAnalytics.DEBUG_ENDPOINT // Use the debug endpoint
            };
            // Test if the GA debug endpoint is returning that the request is valid:
            var e = googleAnalytics.NewEvent("cat1", "action1", "label1", value: 123);
            var res = await googleAnalytics.SendToGA(e).GetResult<GoogleAnalyticsDebugResp>();
            Log.d(JsonWriter.AsPrettyString(res));
            Assert.True(res.hitParsingResult.First().valid, JsonWriter.AsPrettyString(res));
        }

        [Fact]
        public async Task TestSendTimingToGA() {
            var googleAnalytics = new GoogleAnalytics(TEST_APP_KEY, new InMemoryKeyValueStore()) {
                url = GoogleAnalytics.DEBUG_ENDPOINT // Use the debug endpoint
            };
            // Test if the GA debug endpoint is returning that the request is valid:
            var t = googleAnalytics.NewTiming("cat1", "var1", timingInMs: 22);
            var res = await googleAnalytics.SendToGA(t).GetResult<GoogleAnalyticsDebugResp>();
            Log.d(JsonWriter.AsPrettyString(res));
            Assert.True(res.hitParsingResult.First().valid, JsonWriter.AsPrettyString(res));
        }

        [Fact]
        public async Task TestAppFlowToGoogleAnalytics() {
            var tracker = new GoogleAnalytics(TEST_APP_KEY, new InMemoryKeyValueStore());
            AppFlow.AddAppFlowTracker(tracker);

            var t = Log.MethodEntered(); // This will internally notify the AppFlow instance
            await TaskV2.Delay(100);
            Log.MethodDone(t);

            // Check that in the store of the tracker there are now events waiting to be sent:
            var count1 = (await tracker.store.GetAllKeys()).Count();
            Assert.True(count1 > 0);

            await TaskV2.Delay(3000);
            // Check that the events are no longer in the store (sent to Google Analytics):
            var count2 = (await tracker.store.GetAllKeys()).Count();
            Assert.True(count2 < count1, "count2=" + count2 + ", count1=" + count1);
        }

        // See https://developers.google.com/analytics/devguides/collection/protocol/v1/validating-hits#response
        public class GoogleAnalyticsDebugResp {
            public List<HitParsingResult> hitParsingResult { get; set; }
            public List<ParserMessage> parserMessage { get; set; }
        }

        public class HitParsingResult {
            public bool valid { get; set; }
            public string hit { get; set; }
            public List<ParserMessage> parserMessage { get; set; }
        }

        public class ParserMessage {
            public string messageType { get; set; }
            public string description { get; set; }
            public string parameter { get; set; }
            public string messageCode { get; set; }
        }

    }

}