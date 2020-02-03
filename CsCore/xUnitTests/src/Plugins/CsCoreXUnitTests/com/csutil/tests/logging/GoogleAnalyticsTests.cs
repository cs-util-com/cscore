using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using Xunit;

namespace com.csutil.tests.logging {

    public class GoogleAnalyticsTests {

        // https://analytics.google.com/analytics/web/?pli=1#/report/content-event-events/a130164002w221792235p210768996/
        private const string TEST_APP_KEY = "UA-130164002-5";

        [Fact]
        public async Task TestAppFlowToGoogleAnalytics1() {
            var tracker = new GoogleAnalyticsTracker(TEST_APP_KEY, new InMemoryKeyValueStore()) {
                url = GoogleAnalyticsTracker.DEBUG_ENDPOINT // Use the debug endpoint
            };
            // Test if the GA debug endpoint is returning that the request is valid:
            var e1 = new AppFlowEvent() { cat = "cat1", action = "action1" };
            var res1 = await tracker.SendGaEvent(e1, "label1", value: 123).GetResult<GoogleAnalyticsDebugResp>();
            Assert.True(res1.hitParsingResult.First().valid, JsonWriter.AsPrettyString(res1));
        }

        [Fact]
        public async Task TestAppFlowToGoogleAnalytics2() {
            var tracker = new GoogleAnalyticsTracker(TEST_APP_KEY, new InMemoryKeyValueStore());
            AppFlow.instance = tracker;
            Log.MethodEntered(); // This will internally notify the AppFlow instance
            var count1 = (await tracker.store.GetAllKeys()).Count();
            Assert.True(count1 > 0);
            await Task.Delay(3000);
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