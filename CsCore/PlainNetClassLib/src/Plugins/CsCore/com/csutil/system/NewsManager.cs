using com.csutil.keyvaluestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace com.csutil.system {

    public class NewsManager {

        [Obsolete("Use NewManagerViaGSheetsV2 instead")]
        public static NewsManager NewManagerViaGSheets(string apiKey, string sheetId, string sheetName, IKeyValueStore onDeviceEventsStore) {
            var newsDir = EnvironmentV2.instance.GetOrAddTempFolder("NewsManagerCache");
            var gSheetsCache = new FileBasedKeyValueStore(newsDir.GetChildDir("GSheetsData"));
            var newsLocalDataCache = new FileBasedKeyValueStore(newsDir.GetChildDir("LocalData"));
            IKeyValueStore newsStore = new GoogleSheetsKeyValueStore(gSheetsCache, apiKey, sheetId, sheetName);
            if (onDeviceEventsStore != null) {
                newsStore = new DualStore(newsStore, onDeviceEventsStore);
            }
            return new NewsManager(newsLocalDataCache.GetTypeAdapter<News.LocalData>(), newsStore.GetTypeAdapter<News>());
        }
        
        public static NewsManager NewManagerViaGSheetsV2(Uri csvUri, IKeyValueStore onDeviceEventsStore) {
            var newsDir = EnvironmentV2.instance.GetOrAddTempFolder("NewsManagerCache");
            var gSheetsCache = new FileBasedKeyValueStore(newsDir.GetChildDir("GSheetsData"));
            var newsLocalDataCache = new FileBasedKeyValueStore(newsDir.GetChildDir("LocalData"));
            IKeyValueStore newsStore = new GoogleSheetsKeyValueStoreV2(gSheetsCache, csvUri);
            if (onDeviceEventsStore != null) {
                newsStore = new DualStore(newsStore, onDeviceEventsStore);
            }
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
            allNews = allNews.OrderByDescending(x => x.GetDate(true));
            return allNews;
        }

        public async Task MarkNewsAsRead(News news) {
            news.localData = new News.LocalData() { isRead = true };
            await localCache.Set(news.key, news.localData);
        }

    }

    public class News {

        public static News NewLocalNewsEvent(string title, string descr, string url, string urlText, NewsType type = News.NewsType.New, string id = null) {
            if (id == null) { id = GuidV2.NewGuid().ToString(); }
            return new News() {
                key = id,
                title = title,
                description = descr,
                detailsUrl = url,
                detailsUrlText = urlText,
                date = "" + DateTimeV2.UtcNow.ToReadableString_ISO8601(),
                type = EnumUtil.GetEntryName(type)
            };
        }

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

        public DateTime GetDate(bool returnUtcNowIfDateNull) {
            if (returnUtcNowIfDateNull && date == null) { return DateTimeV2.UtcNow; }
            return DateTimeV2.ParseUtc(date);
        }

        public class LocalData {
            public bool isRead { get; set; }
        }
    }

}