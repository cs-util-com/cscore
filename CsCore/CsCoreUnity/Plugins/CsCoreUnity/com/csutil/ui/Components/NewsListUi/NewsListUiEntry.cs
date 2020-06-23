using System;
using System.Threading.Tasks;
using com.csutil.system;
using com.csutil.ui.Components;
using ReuseScroller;
using UnityEngine.UI;

namespace com.csutil.ui {

    class NewsListUiEntry : BaseCell<News> {

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
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
        public VerticalPositionListener listener;
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

        private NewsListUi newsListUi;

        public override void UpdateContent(News item) {

            if (newsListUi == null) { newsListUi = GetComponentInParent<NewsListUi>(); }

            typeIndicator.color = newsListUi.GetColorFor(item);
            SetText(typeText, item.type);

            LoadImageFromUrl(image, item.imageUrl);
            LoadImageFromUrl(thumbnail, item.thumbnailUrl);

            SetText(dateText, item.GetDate().ToLocalUiString());
            SetText(titleText, item.title);
            SetText(descrText, item.description);

            SetText(detailsUrlText, item.detailsUrlText);

            var hasDetailsUrl = item.detailsUrl.IsNullOrEmpty();
            detailsUrlButton.gameObject.SetActiveV2(!hasDetailsUrl);
            if (!hasDetailsUrl) {
                detailsUrlButton.SetOnClickAction(_ => newsListUi.OnItemClicked(item));
                titleButton.SetOnClickAction(_ => newsListUi.OnItemClicked(item));
            } else { // No URL so only mark as read when clicked:
                titleButton.SetOnClickAction(_ => newsListUi.MarkAsRead(item));
            }

            var isItemRead = item.localData != null && item.localData.isRead;
            listener.enabled = !isItemRead;
            SetTextColorsTransparent(isItemRead);
            listener.onScreen = (percent) => {
                if (!isItemRead && percent > newsListUi.markAsReadVerticalPosThreshold) {
                    isItemRead = true;
                    newsListUi.MarkAsRead(item).LogOnError();
                    SetTextColorsTransparent(isItemRead);
                }
            };

        }

        private void SetTextColorsTransparent(bool isItemRead) {
            float textTransparency = isItemRead ? newsListUi.markasReadTransparency : 1;
            dateText.color = dateText.color.WithAlpha(textTransparency);
            typeText.color = typeText.color.WithAlpha(textTransparency);
            titleText.color = titleText.color.WithAlpha(textTransparency);
            descrText.color = descrText.color.WithAlpha(textTransparency);
            detailsUrlText.color = detailsUrlText.color.WithAlpha(textTransparency);
        }

        private void LoadImageFromUrl(Image targetImage, string urlToLoad) {
            AssertV2.IsNotNull(targetImage, "targetImage");
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
