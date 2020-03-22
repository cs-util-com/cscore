using System;
using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {

    public class JsonFeedTests {

        public JsonFeedTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var feed = await JsonFeed.Get(new Uri("https://hnrss.org/frontpage.jsonfeed"));
            Assert.NotEmpty(feed.items);
            foreach (var item in feed.items) { Log.d(item.title + " - " + item.url); }
        }

    }

}
