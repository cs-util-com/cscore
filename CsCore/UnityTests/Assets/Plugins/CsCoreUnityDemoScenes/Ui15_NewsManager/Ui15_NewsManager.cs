using System;
using com.csutil.system;
using System.Collections;
using System.Threading.Tasks;
using com.csutil.ui;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model.usagerules;

namespace com.csutil.tests {

    public class Ui15_NewsManager : UnitTestMono {

        public override IEnumerator RunTest() {

            // See https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            var uri = new Uri("https://docs.google.com/spreadsheets/d/e/2PACX-1vQhCWZHOifEU5liS9x_H6BA6BcpBHOHHc_28VC3oFM0xpkTMTFfn8D7MF_PUKQatyKxQFphTfSWXeDg/pub?output=csv");
            
            var onDeviceEventsStore = new InMemoryKeyValueStore();
            yield return AddAFewLocalNewsEvents(onDeviceEventsStore).AsCoroutine();

            NewsManager manager = NewsManager.NewManagerViaGSheetsV2(uri, onDeviceEventsStore);
            yield return LoadAllNews(manager).AsCoroutine();

        }

        private static async Task AddAFewLocalNewsEvents(InMemoryKeyValueStore onDeviceEventsStore) {

            var dir = EnvironmentV2.instance.GetNewInMemorySystem();
            var a = new LocalAnalyticsV2(dir);
            // Set up a few simple usage rules to generate local news events from if they are true:
            var appUsed1DayRule = a.NewAppUsedXDaysRule(days: 1);
            var featureNeverUsedRule = a.NewFeatureNotUsedXTimesRule("feature1", times: 1);

            var appNotUsedLast5Days = a.NewAppUsedInTheLastXDaysRule(5);
            var userCameBackRule = a.NewConcatRule(appUsed1DayRule, appNotUsedLast5Days);

            var url = "https://github.com/cs-util-com/cscore";
            var urlText = "Show details..";

            var x = News.NewLocalNewsEvent("Entry added via code", "This entry was added via code and not received from the remote Google Sheet", url, urlText);
            await onDeviceEventsStore.Set(x.key, x);
            
            if (await appUsed1DayRule.isTrue()) {
                var n = News.NewLocalNewsEvent("Achievement Unlocked", "You used the app 1 day", url, urlText);
                await onDeviceEventsStore.Set(n.key, n);
            }
            if (await featureNeverUsedRule.isTrue()) {
                var n = News.NewLocalNewsEvent("Did you know you can do feature1?", "Feature 1 is the best", url, urlText);
                await onDeviceEventsStore.Set(n.key, n);
            }
            if (await userCameBackRule.isTrue()) {
                var n = News.NewLocalNewsEvent("You did not use the app for a while", "How dare you", url, urlText);
                await onDeviceEventsStore.Set(n.key, n);
            }
        }

        private async Task LoadAllNews(NewsManager manager) {
            var newsListUi = gameObject.GetComponentInChildren<NewsListUi>();
            newsListUi.newsManager = manager;
            await newsListUi.LoadNews();
        }

    }

}
