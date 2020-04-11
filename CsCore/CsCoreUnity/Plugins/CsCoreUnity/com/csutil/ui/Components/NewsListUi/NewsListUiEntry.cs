using System;
using System.Threading.Tasks;
using com.csutil.system;
using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.ui {

    class NewsListUiEntry : BaseCell<News> {

        public Text dateText;
        public Text typeText;
        public Text titleText;
        public Text descrText;
        public Text detailsUrlText;
        public Graphic typeIndicator;
        public Image image;
        public Image thumbnail;
        public Button titleButton;
        public Button detailsUrlButton;
        private NewsListUi newsListUi;

        public override void UpdateContent(News item) {

            if (newsListUi == null) { newsListUi = GetComponentInParent<NewsListUi>(); }

            typeIndicator.color = newsListUi.GetColorFor(item);
            SetText(typeText, item.type);

            LoadImageFromUrl(image, item.imageUrl);
            LoadImageFromUrl(thumbnail, item.thumbnailUrl);

            SetText(dateText, item.GetDate().ToReadableString());
            SetText(titleText, item.title);
            SetText(descrText, item.description);

            SetText(detailsUrlText, item.detailsUrlText);

            detailsUrlButton.gameObject.SetActiveV2(!item.detailsUrl.IsNullOrEmpty());
            if (!item.detailsUrl.IsNullOrEmpty()) {
                detailsUrlButton.SetOnClickAction(_ => newsListUi.OnItemClicked(item));
                titleButton.SetOnClickAction(_ => newsListUi.OnItemClicked(item));
            }

        }

        private void LoadImageFromUrl(Image targetImage, string urlToLoad) {
            AssertV2.NotNull(targetImage, "targetImage");
            var isUrlEmpty = urlToLoad.IsNullOrEmpty();
            targetImage.gameObject.GetParent().SetActiveV2(!isUrlEmpty);
            if (!isUrlEmpty) { targetImage.LoadFromUrl(urlToLoad).OnError(LogImageLoadError); }
        }

        private Task LogImageLoadError(Exception e) { Log.w("Image Load Error: " + e); return Task.FromException(e); }

        private static void SetText(Text target, string text) {
            if (target == null) { return; }
            target.gameObject.SetActiveV2(!text.IsNullOrEmpty());
            target.text = text;
        }

    }

}
