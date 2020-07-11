using com.csutil.system;
using System.Collections;
using System.Threading.Tasks;
using com.csutil.ui;
using com.csutil.keyvaluestore;

namespace com.csutil.tests {

    public class Ui15_NewsManager : UnitTestMono {

        public override IEnumerator RunTest() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            var sheetId = "1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A";
            var sheetName = "MySheet1"; // Has to match the sheet name

            var onDeviceEventsStore = new InMemoryKeyValueStore();
            yield return AddAFewLocalNewsEvents(onDeviceEventsStore).AsCoroutine();

            NewsManager manager = NewsManager.NewManagerViaGSheets(apiKey, sheetId, sheetName, onDeviceEventsStore);
            yield return LoadAllNews(manager).AsCoroutine();

        }

        private static async Task AddAFewLocalNewsEvents(InMemoryKeyValueStore onDeviceEventsStore) {
            var url = "https://github.com/cs-util-com/cscore";
            var urlText = "Show details..";
            var n = News.NewLocalNewsEvent("Achivement Unlocked", "You just started the app", url, urlText);
            await onDeviceEventsStore.Set(n.key, n);
            await TaskV2.Delay(10);
            var n2 = News.NewLocalNewsEvent("Feature X now available", "You can now do X", url, urlText);
            await onDeviceEventsStore.Set(n2.key, n2);
            await TaskV2.Delay(10);
            var n3 = News.NewLocalNewsEvent("Feature Y now available", "You can now do Y", url, urlText);
            await onDeviceEventsStore.Set(n3.key, n3);
        }

        private async Task LoadAllNews(NewsManager manager) {
            var newsListUi = gameObject.GetComponentInChildren<NewsListUi>();
            newsListUi.newsManager = manager;
            await newsListUi.LoadNews();
        }

    }

}
