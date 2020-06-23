using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.model;
using com.csutil.system;
using ReuseScroller;
using UnityEngine;

namespace com.csutil.ui {

    public class NewsListUi : BaseController<News> {

        public ColorDict colors = InitDefaultColors(); // Init with default color suggestions
        public float markAsReadVerticalPosThreshold = 0.8f;
        public float markasReadTransparency = 0.3f;
        public bool showOnlyUnread = false;
        public NewsManager newsManager; // protected because its accessed by the list entries

        public async Task LoadNews() {
            AssertV2.IsNotNull(newsManager, "newsManager");
            var allNews = showOnlyUnread ? await newsManager.GetAllUnreadNews() : await newsManager.GetAllNews();
            this.CellData = allNews.ToList(); // This will trigger showing the list entries
        }

        public Color GetColorFor(News item) {
            if (colors.TryGetValue(item.GetNewsType(), out Color result)) { return result; }
            return Color.white;
        }

        public virtual void OnItemClicked(News clickedItem) {
            Application.OpenURL(clickedItem.detailsUrl);
            MarkAsRead(clickedItem).LogOnError();
        }

        [Serializable]
        public class ColorDictEntry : SerializableEntry<News.NewsType, Color> { }
        [Serializable]
        public class ColorDict : SerializableDictionary<News.NewsType, Color, ColorDictEntry> { }

        // Compact notation via dict and can be edited in inspector to adjust default values
        private static ColorDict InitDefaultColors() {
            var colors = new ColorDict(); // Serializable subclass of normal Dictionary
            colors.Add(News.NewsType.Blog, ColorUtil.HexStringToColor("FBAE4E"));
            colors.Add(News.NewsType.Announcement, ColorUtil.HexStringToColor("FFAE1B"));
            colors.Add(News.NewsType.ComingSoon, ColorUtil.HexStringToColor("59D457"));
            colors.Add(News.NewsType.Beta, ColorUtil.HexStringToColor("FF5A80"));
            colors.Add(News.NewsType.New, ColorUtil.HexStringToColor("FF5A80"));
            colors.Add(News.NewsType.Improvement, ColorUtil.HexStringToColor("71C4FF"));
            colors.Add(News.NewsType.Warning, ColorUtil.HexStringToColor("EB5756"));
            colors.Add(News.NewsType.Fix, ColorUtil.HexStringToColor("8482F5"));
            return colors;
        }

        internal async Task MarkAsRead(News news) {
            await newsManager.MarkNewsAsRead(news);
            ReloadData();
        }
    }

}