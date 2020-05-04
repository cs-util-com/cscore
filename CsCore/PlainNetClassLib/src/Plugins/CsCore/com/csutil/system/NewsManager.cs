using com.csutil.keyvaluestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace com.csutil.system {

    public class NewsManager {

        public static NewsManager NewManagerViaGSheets(string apiKey, string sheetId, string sheetName) {
            var newsDir = EnvironmentV2.instance.GetOrAddTempFolder("NewsManagerCache");
            var gSheetsCache = new FileBasedKeyValueStore(newsDir.GetChildDir("GSheetsData"));
            var newsLocalDataCache = new FileBasedKeyValueStore(newsDir.GetChildDir("LocalData"));
            IKeyValueStore newsStore = new GoogleSheetsKeyValueStore(gSheetsCache, apiKey, sheetId, sheetName);
            return new NewsManager(newsLocalDataCache.GetTypeAdapter<News.LocalData>(), newsStore.GetTypeAdapter<News>());
        }

        private KeyValueStoreTypeAdapter<News.LocalData> localCache;
        private KeyValueStoreTypeAdapter<News> newsStore;

        public NewsManager(KeyValueStoreTypeAdapter<News.LocalData> localCache, KeyValueStoreTypeAdapter<News> newsStore) {
            this.localCache = localCache;
            this.newsStore = newsStore;
        }

        public async Task<IEnumerable<News>> GetAllUnreadNews() {
            IEnumerable<News> allNews = await GetAllNews();
            return allNews.Filter(n => n.localData == null || !n.localData.isRead);
        }

        public async Task<IEnumerable<News>> GetAllNews() {
            IEnumerable<News> allNews = await newsStore.GetAll();
            allNews = await allNews.MapAsync(async news => { // Include localData from the cache:
                news.localData = await localCache.Get(news.key, null);
                return news;
            });
            allNews = allNews.OrderByDescending(x => x.GetDate());
            return allNews;
        }

        public async Task MarkNewsAsRead(News news) {
            news.localData = new News.LocalData() { isRead = true };
            await localCache.Set(news.key, news.localData);
        }

    }

    public class News {

        public string key { get; set; }
        public string title { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public string thumbnailUrl { get; set; }
        public string imageUrl { get; set; }
        public string detailsUrl { get; set; }
        public string detailsUrlText { get; set; }
        public LocalData localData { get; set; }

        public enum NewsType {
            Blog, Announcement, ComingSoon, Beta, New, Improvement, Warning, Fix, Unknown
        }

        public NewsType GetNewsType() { return EnumUtil.TryParse(type, NewsType.Unknown); }

        public DateTime GetDate() { return DateTimeV2.ParseUtc(date); }

        public class LocalData {
            public bool isRead { get; set; }
        }
    }

}