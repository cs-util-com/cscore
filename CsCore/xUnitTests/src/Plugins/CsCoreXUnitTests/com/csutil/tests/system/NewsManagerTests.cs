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
            IKeyValueStore newsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);

            var newsLocalDataStore = new InMemoryKeyValueStore().GetTypeAdapter<News.LocalData>();
            NewsManager manager = new NewsManager(newsLocalDataStore, newsStore.GetTypeAdapter<News>());

            IEnumerable<News> allNews = await manager.GetAllNews();
            var news1 = allNews.First();
            Assert.NotNull(news1);
            Assert.Equal("Coming Soon", news1.type);
            Assert.Equal(News.NewsType.ComingSoon, news1.GetNewsType());
            Assert.True(news1.GetDate(false).IsUtc());

            // Mark that the user has read the news:
            await manager.MarkNewsAsRead(news1);
            Assert.True(allNews.First().localData.isRead);
            Assert.True((await manager.GetAllNews()).First().localData.isRead);
            Assert.True((await newsLocalDataStore.Get(news1.key, null)).isRead);

            IEnumerable<News> unreadNews = await manager.GetAllUnreadNews();
            Assert.Contains(news1, allNews);
            Assert.DoesNotContain(news1, unreadNews);

        }

        [Fact]
        public async Task ExampleUsage2() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            var sheetId = "1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A";
            var sheetName = "MySheet1"; // Has to match the sheet name
            IKeyValueStore onlineNewsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);

            IKeyValueStore onDeviceEventsStore = new InMemoryKeyValueStore();
            IKeyValueStore newsStore = new DualStore(onlineNewsStore, onDeviceEventsStore);

            var newsLocalDataStore = new InMemoryKeyValueStore().GetTypeAdapter<News.LocalData>();
            NewsManager manager = new NewsManager(newsLocalDataStore, newsStore.GetTypeAdapter<News>());

            var title = "First App Start";
            var descr = "You just started the app the first time!";
            var url = "https://github.com/cs-util-com/cscore";
            var urlText = "Show details..";
            News n = News.NewLocalNewsEvent(title, descr, url, urlText);
            await onDeviceEventsStore.Set(n.key, n);

            IEnumerable<News> allNews = await manager.GetAllNews();
            Assert.Contains(allNews, x => x.title == title);

            Log.d(JsonWriter.AsPrettyString(allNews));

            IEnumerable<News> unreadNews = await manager.GetAllUnreadNews();
            Assert.Contains(unreadNews, x => x.title == title);

        }

    }

}