using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model.usagerules;
using Xunit;

namespace com.csutil.integrationTests.model {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class FeatureUsageRuleSystemTests {

        public FeatureUsageRuleSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1New() {
            // Create an analytics system and fill it with feature usage events:
            var analytics = CreateLocalAnalyticsSystemV2();
            await ExampleUsage1(analytics);
        }
        
        [Obsolete]
        [Fact]
        public async Task ExampleUsage1Old() {
            // Create an analytics system and fill it with feature usage events:
            var analytics = CreateLocalAnalyticsSystem();
            await ExampleUsage1(analytics);
        }
        
        private async Task ExampleUsage1(ILocalAnalytics analytics) {
            var t = new MockDateTimeV2();
            IoC.inject.SetSingleton<IClock>(t, overrideExisting: true);

            string featureId = "feature1";

            {
                var startDate = DateTimeV2.ParseUtc("2011-01-01");
                var endDate = DateTimeV2.ParseUtc("2011-01-19");

                SimulateUsage(featureId, 100, t, startDate, endDate);
                await AssertFeatureUsageDetected(analytics, featureId, 100);
            }
            t.mockUtcNow = DateTimeV2.ParseUtc("2011-02-01");
            await TestAllRules1(analytics, featureId);

            { // Simulate more usage in the next 5 days:
                var startDate = DateTimeV2.ParseUtc("2011-01-20");
                var endDate = DateTimeV2.ParseUtc("2011-01-25");

                SimulateUsage(featureId, 100, t, startDate, endDate);
                await AssertFeatureUsageDetected(analytics, featureId, 200);
            }
            // Simulate a month without any usage:
            t.mockUtcNow = DateTimeV2.ParseUtc("2011-03-01");
            await TestAllRules2(analytics, featureId);

            { // Simulate that a usage notification is shown and test the related rule:
                var notificationId = "notification1";
                var daysAgo = 20;

                t.mockUtcNow = DateTimeV2.ParseUtc("2011-03-01");
                // Simulate that notification1 is shown to the user (e.g. by the usageRule system):
                AppFlow.TrackEvent(EventConsts.catUsage, EventConsts.SHOW + "_" + notificationId);

                t.mockUtcNow = DateTimeV2.ParseUtc("2011-03-02"); // Simulate a day passing by
                UsageRule notificationMinXDaysOld = analytics.NewNotificationMinXDaysOldRule(notificationId, daysAgo);
                Assert.False(await notificationMinXDaysOld.isTrue());
                Assert.False(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));

                t.mockUtcNow = DateTimeV2.ParseUtc("2011-03-25"); // Simulate more time passing by
                Assert.True(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));
                Assert.True(await notificationMinXDaysOld.isTrue());

                // Simulate a second show of the notification:
                AppFlow.TrackEvent(EventConsts.catUsage, EventConsts.SHOW + "_" + notificationId);
                Assert.False(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));
                Assert.False(await notificationMinXDaysOld.isTrue());
                
                t.mockUtcNow = DateTimeV2.ParseUtc("2011-04-15"); // Simulate more time passing by
                Assert.True(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));
                Assert.True(await notificationMinXDaysOld.isTrue());
            }

        }

        async Task TestAllRules1(ILocalAnalytics analytics, string featureId) {

            var daysUsed = 20;
            var timesUsed = 200;

            UsageRule featureUsedXDays = analytics.NewFeatureUsedXDaysRule(featureId, daysUsed);
            Assert.False(await featureUsedXDays.isTrue());
            Assert.False(await featureUsedXDays.IsFeatureUsedXDays(analytics)); // Used by .isTrue

            UsageRule featureNotUsedXDays = analytics.NewFeatureNotUsedXDaysRule(featureId, daysUsed);
            Assert.True(await featureNotUsedXDays.isTrue());

            UsageRule appNotUsedXDays = analytics.NewAppNotUsedXDaysRule(daysUsed);
            Assert.True(await appNotUsedXDays.isTrue());

            UsageRule featureNotUsedXTimes = analytics.NewFeatureNotUsedXTimesRule(featureId, timesUsed);
            Assert.True(await featureNotUsedXTimes.isTrue());

            UsageRule appUsedInTheLastXDays = analytics.NewAppUsedInTheLastXDaysRule(daysUsed);
            Assert.True(await appUsedInTheLastXDays.isTrue());

            UsageRule appNotUsedInTheLastXDays = analytics.NewAppNotUsedInTheLastXDaysRule(daysUsed);
            Assert.False(await appNotUsedInTheLastXDays.isTrue());

            UsageRule featureUsedInTheLastXDays = analytics.NewFeatureUsedInTheLastXDaysRule(featureId, daysUsed);
            Assert.True(await featureUsedInTheLastXDays.isTrue());

            { // Compose a more complex usage rule out of multiple rules:
                UsageRule appUsedXDays = analytics.NewAppUsedXDaysRule(daysUsed);
                Assert.False(await appUsedXDays.isTrue());

                UsageRule featureUsedXTimes = analytics.NewFeatureUsedXTimesRule(featureId, timesUsed);
                Assert.False(await featureUsedXTimes.isTrue());

                UsageRule featureNotUsedInTheLastXDays = analytics.NewFeatureNotUsedInTheLastXDaysRule(featureId, daysUsed);
                Assert.False(await featureNotUsedInTheLastXDays.isTrue());

                UsageRule featureNotUsedAnymoreRule = analytics.NewConcatRule(
                    appUsedXDays, featureUsedXTimes, featureNotUsedInTheLastXDays
                );
                Assert.False(await featureNotUsedAnymoreRule.isTrue());

                UsageRule clone = featureNotUsedAnymoreRule.DeepCopyViaJson();
                clone.SetupUsing(analytics);
                Assert.False(await clone.isTrue());

            }
        }

        /// <summary> Same rules as in TestAllRules1 but inverted assertions </summary>
        async Task TestAllRules2(ILocalAnalytics analytics, string featureId) {

            var daysUsed = 20;
            var timesUsed = 50;

            UsageRule featureUsedXDays = analytics.NewFeatureUsedXDaysRule(featureId, daysUsed);
            Assert.True(await featureUsedXDays.isTrue());
            Assert.True(await featureUsedXDays.IsFeatureUsedXDays(analytics)); // Used by .isTrue

            UsageRule featureNotUsedXDays = analytics.NewFeatureNotUsedXDaysRule(featureId, daysUsed);
            Assert.False(await featureNotUsedXDays.isTrue());

            UsageRule appNotUsedXDays = analytics.NewAppNotUsedXDaysRule(daysUsed);
            Assert.False(await appNotUsedXDays.isTrue());

            UsageRule featureNotUsedXTimes = analytics.NewFeatureNotUsedXTimesRule(featureId, timesUsed);
            Assert.False(await featureNotUsedXTimes.isTrue());

            UsageRule appUsedInTheLastXDays = analytics.NewAppUsedInTheLastXDaysRule(daysUsed);
            bool appWasUsedInTheLastXDays = await appUsedInTheLastXDays.isTrue();
            { // Asserting this in the unit test is disabled to have this example code here act as a functioning regression test
                //Assert.False(appWasUsedInTheLastXDays);
                if (appWasUsedInTheLastXDays) {
                    Log.e("appWasUsedInTheLastXDays which should only be true if the test is executed in parallel with "
                        + "many other tests that also use the LocalAnalytics system");
                }
            }

            UsageRule appNotUsedInTheLastXDays = analytics.NewAppNotUsedInTheLastXDaysRule(daysUsed);
            bool appWasNOTUsedInTheLastXDays = await appNotUsedInTheLastXDays.isTrue();
            { // Asserting this in the unit test is disabled to have this example code here act as a functioning regression test
                if (!appWasNOTUsedInTheLastXDays) {
                    Log.e("appWasNOTUsedInTheLastXDays which should only be false if the test is executed in parallel with "
                        + "many other tests that also use the LocalAnalytics system");
                }
            }

            UsageRule featureUsedInTheLastXDays = analytics.NewFeatureUsedInTheLastXDaysRule(featureId, daysUsed);
            Assert.False(await featureUsedInTheLastXDays.isTrue());

            { // Compose a more complex usage rule out of multiple rules:
                UsageRule appUsedXDays = analytics.NewAppUsedXDaysRule(daysUsed);
                Assert.True(await appUsedXDays.isTrue());

                UsageRule featureUsedXTimes = analytics.NewFeatureUsedXTimesRule(featureId, timesUsed);
                Assert.True(await featureUsedXTimes.isTrue());

                UsageRule featureNotUsedInTheLastXDays = analytics.NewFeatureNotUsedInTheLastXDaysRule(featureId, daysUsed);
                Assert.True(await featureNotUsedInTheLastXDays.isTrue());

                UsageRule featureNotUsedAnymoreRule = analytics.NewConcatRule(
                    appUsedXDays, featureUsedXTimes, featureNotUsedInTheLastXDays
                );
                Assert.True(await featureNotUsedAnymoreRule.isTrue());

                UsageRule clone = featureNotUsedAnymoreRule.DeepCopyViaJson();
                clone.SetupUsing(analytics);
                Assert.True(await clone.isTrue());

            }

        }

        [Fact]
        public async Task ExampleUsage2New() {
            var analytics = CreateLocalAnalyticsSystemV2();
            await ExampleUsage2(analytics);
        }
        
        [Obsolete]
        [Fact]
        public async Task ExampleUsage2Old() {
            var analytics = CreateLocalAnalyticsSystem();
            await ExampleUsage2(analytics);
        }
        
        private static async Task ExampleUsage2(ILocalAnalytics analytics) { // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = await IoC.inject.GetAppSecrets().GetSecret("GoogleSheetsV4Key");
            // https://docs.google.com/spreadsheets/d/1rl1vi-LUhOgoY_QrMJsm2UE0SdiL4EbOtLwfNPsavxQ contains the sheetId:
            var sheetId = "1rl1vi-LUhOgoY_QrMJsm2UE0SdiL4EbOtLwfNPsavxQ";
            var sheetName = "UsageRules1"; // Has to match the sheet name
            var source = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);
            var store = source.GetTypeAdapter<UsageRule>();

            IEnumerable<UsageRule> rules = await store.GetRulesInitialized(analytics);
            foreach (var rule in rules) {
                if (await rule.isTrue()) {
                    Log.d(JsonWriter.AsPrettyString(rule)); // TODO
                }
            }
            Assert.Single(rules.Filter(r => !r.andRules.IsNullOrEmpty()));

        }

        [Fact]
        public async Task ExampleUsage3_NpsScoreSystemNew() {
            var analytics = CreateLocalAnalyticsSystemV2();
            await ExampleUsage3_NpsScoreSystem(analytics);
        }
        
        [Obsolete]
        /// <summary> See https://github.com/cs-util-com/cscore/issues/54 </summary>
        [Fact]
        public async Task ExampleUsage3_NpsScoreSystemOld() {
            var analytics = CreateLocalAnalyticsSystem();
            await ExampleUsage3_NpsScoreSystem(analytics);
        }
        
        private static async Task ExampleUsage3_NpsScoreSystem(ILocalAnalytics analytics) {
            var t = new MockDateTimeV2();
            IoC.inject.SetSingleton<IClock>(t, overrideExisting: true);
            t.mockUtcNow = DateTimeV2.ParseUtc("01.02.2011");


            // The user must have used the app at least on 3 different days before the NPS score should be collected
            UsageRule appUsedInTheLastXDays = analytics.NewAppUsedXDaysRule(3);
            Assert.False(await appUsedInTheLastXDays.isTrue());

            t.mockUtcNow = DateTimeV2.ParseUtc("02.02.2011");
            AppFlow.TrackEvent("someEventCategory1", "someAction1");
            Assert.False(await appUsedInTheLastXDays.isTrue());

            t.mockUtcNow = DateTimeV2.ParseUtc("03.02.2011");
            AppFlow.TrackEvent("someEventCategory1", "someAction1");
            Assert.False(await appUsedInTheLastXDays.isTrue());

            t.mockUtcNow = DateTimeV2.ParseUtc("04.02.2011");
            AppFlow.TrackEvent("someEventCategory1", "someAction1");
            Assert.True(await appUsedInTheLastXDays.isTrue());
            
            // TODO make a load test that simulates app usage over many years with 1000 events per day 
            // NewAppUsedXDaysRule uses an analytics.GetAll() call that would load all files of the entire keyvaluestore which would be very slow??
            
        }

        [Obsolete("Use v2",true)]
        private static ILocalAnalytics CreateLocalAnalyticsSystem() {
            LocalAnalytics analytics = new LocalAnalytics(new InMemoryKeyValueStore());
            analytics.createStoreFor = (_) => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>();
            // Setup the AppFlow logic to use the LocalAnalytics system:
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));
            return analytics;
        }
        
        private static ILocalAnalytics CreateLocalAnalyticsSystemV2() {
            var dir = EnvironmentV2.instance.GetNewInMemorySystem();
            LocalAnalyticsV3 analytics = new LocalAnalyticsV3(dir);
            // Setup the AppFlow logic to use the LocalAnalytics system:
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));
            return analytics;
        }

        void SimulateUsage(string featureId, int evCount, MockDateTimeV2 t, DateTime start, DateTime end) {
            double s = start.ToUnixTimestampUtc();
            double durationInMs = (end - start).TotalMilliseconds;
            Assert.NotEqual(0, durationInMs);

            for (int i = 0; i < evCount; i++) {
                double percent = i / (double)evCount;
                Assert.InRange(percent, 0, 1);
                t.mockUtcNow = DateTimeV2.NewDateTimeFromUnixTimestamp((long)(s + percent * durationInMs));
                AppFlow.TrackEvent(featureId, EventConsts.START);
            }
        }

        async Task AssertFeatureUsageDetected(ILocalAnalytics analytics, string featureId, int expectedCount) {
            var featureEventStore = analytics.categoryStores[featureId];
            var allFeatureEvents = await featureEventStore.GetAll();
            Assert.Equal(expectedCount, allFeatureEvents.Count());
            var allStartEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
            Assert.Equal(expectedCount, allStartEvents.Count());
        }

        private class MockDateTimeV2 : DateTimeV2 {
            public DateTime mockUtcNow;
            public override DateTime GetUtcNow() { return mockUtcNow; }
        }

        [Fact] // Event tracking should by default not interrupt the normal application flow:
        public void TestErrorInTrackEvent() {
            var tracker = new AppFlowThatThrowsErrors();
            AppFlow.AddAppFlowTracker(tracker);
            // Even though TrackEvent does not work correctly the application flow is not interrupted: 
            AppFlow.TrackEvent("category 1", "action 1");
            IoC.inject.Get<IAppFlow>(this).TrackEvent("category 1", "action 2");
            // Directly using the tracker would not protect against the inner exception:
            Assert.Throws<InvalidOperationException>(() => tracker.TrackEvent("category 1", "action 3"));
        }

        private class AppFlowThatThrowsErrors : IAppFlow {
            public void TrackEvent(string category, string action, params object[] args) {
                throw new InvalidOperationException();
            }
        }

    }

}