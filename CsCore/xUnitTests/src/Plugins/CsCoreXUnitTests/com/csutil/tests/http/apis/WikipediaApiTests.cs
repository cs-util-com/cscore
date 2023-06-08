using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis.wikipedia;
using Xunit;

namespace com.csutil.tests.http {

    public class WikipediaApiTests {

        public WikipediaApiTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var exampleTopics = new List<string> {
                "Operation Timber Sycamore", "Operation Fast and Furious", "MKUltra",
                "COINTELPRO", "Bilderberg Group", "Bohemian Grove", "Operation Sea-Spray", "Order 322",
                "Operation Mockingbird", "Operation Northwoods", "Operation Paperclip", "Lavon Affair",
                "US-984XN", "XKeyscore", "Tuskegee Experiment"
            };
            var randomTopic = new Random().ShuffleEntries(exampleTopics).First();
            Wikipedia.SearchResult result = await Wikipedia.Search(randomTopic);
            Wikipedia.Article article = await Wikipedia.GetArticle(result.query.search.First().pageid);
            Log.d(JsonWriter.AsPrettyString(article));
        }

    }

}