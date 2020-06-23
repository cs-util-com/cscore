using com.csutil.system;
using System.Collections;
using System.Threading.Tasks;
using com.csutil.ui;

namespace com.csutil.tests {

    public class Ui15_NewsManager : UnitTestMono {

        public override IEnumerator RunTest() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            var sheetId = "1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A";
            var sheetName = "MySheet1"; // Has to match the sheet name

            NewsManager manager = NewsManager.NewManagerViaGSheets(apiKey, sheetId, sheetName);
            yield return LoadAllNews(manager).AsCoroutine();

        }

        private async Task LoadAllNews(NewsManager manager) {
            var newsListUi = gameObject.GetComponentInChildren<NewsListUi>();
            newsListUi.newsManager = manager;
            await newsListUi.LoadNews();
        }

    }

}
