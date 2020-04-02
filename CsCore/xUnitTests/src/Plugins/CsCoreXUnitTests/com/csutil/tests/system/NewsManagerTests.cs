using com.csutil.keyvaluestore;
using com.csutil.system;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.system {

    public class NewsManagerTests {

        public NewsManagerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            var sheetId = "1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var localCache = new InMemoryKeyValueStore();
            IKeyValueStore newsStore = new GoogleSheetsKeyValueStore(localCache, apiKey, sheetId, sheetName);

            var news1 = await newsStore.Get<News>("News1", null);
            Assert.NotNull(news1);
            Assert.Equal("Warning", news1.type);
            Assert.Equal(News.NewsType.Warning, news1.GetNewsType());
            var news = await newsStore.GetAll<News>();

            foreach (var n in news) { Log.d(n.title); }

        }

    }

}