using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model.usagerules;
using Xunit;

namespace com.csutil.tests.model {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class FeatureUsageRuleSystemTests {

        public FeatureUsageRuleSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // Create an analytics system and fill it with feature usage events:
            var analytics = CreateLocalAnalyticsSystem();

            var t = new MockDateTimeV2();
            IoC.inject.SetSingleton<DateTimeV2>(t);

            string featureId = "feature1";

            {
                var startDate = DateTimeV2.ParseUtc("01.01.2011");
                var endDate = DateTimeV2.ParseUtc("19.01.2011");

                SimulateUsage(featureId, 100, t, startDate, endDate);
                await AssertFeatureUsageDetected(analytics, featureId, 100);
            }
            t.mockUtcNow = DateTimeV2.ParseUtc("01.02.2011");
            await TestAllRules1(analytics, featureId);

            { // Simulate more usage in the next 5 days:
                var startDate = DateTimeV2.ParseUtc("20.01.2011");
                var endDate = DateTimeV2.ParseUtc("25.01.2011");

                SimulateUsage(featureId, 100, t, startDate, endDate);
                await AssertFeatureUsageDetected(analytics, featureId, 200);
            }
            // Simulate a month without any usage:
            t.mockUtcNow = DateTimeV2.ParseUtc("01.03.2011");
            await TestAllRules2(analytics, featureId);

            { // Simulate that a usage notification is shown and test the related rule:
                var notificationId = "notification1";
                var daysAgo = 20;

                t.mockUtcNow = DateTimeV2.ParseUtc("01.03.2011");
                // Simulate that notification1 is shown to the user (e.g. by the usageRule system):
                AppFlow.TrackEvent(EventConsts.catUsage, EventConsts.SHOW + "_" + notificationId);

                t.mockUtcNow = DateTimeV2.ParseUtc("02.03.2011"); // Simulate a day passing by
                UsageRule notificationMinXDaysOld = analytics.NewNotificationMinXDaysOldRule(notificationId, daysAgo);
                Assert.False(await notificationMinXDaysOld.isTrue());
                Assert.False(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));

                t.mockUtcNow = DateTimeV2.ParseUtc("25.03.2011"); // Simulate more time passing by
                Assert.True(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));
                Assert.True(await notificationMinXDaysOld.isTrue());

                // Simulate a second show of the notification:
                AppFlow.TrackEvent(EventConsts.catUsage, EventConsts.SHOW + "_" + notificationId);
                Assert.True(await notificationMinXDaysOld.IsNotificationMinXDaysOld(analytics));
                Assert.True(await notificationMinXDaysOld.isTrue());
            }

        }

        async Task TestAllRules1(LocalAnalytics analytics, string featureId) {

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
        async Task TestAllRules2(LocalAnalytics analytics, string featureId) {

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
            Assert.False(await appUsedInTheLastXDays.isTrue());

            UsageRule appNotUsedInTheLastXDays = analytics.NewAppNotUsedInTheLastXDaysRule(daysUsed);
            Assert.True(await appNotUsedInTheLastXDays.isTrue());

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
        public async Task ExampleUsage2() {
            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1rl1vi-LUhOgoY_QrMJsm2UE0SdiL4EbOtLwfNPsavxQ contains the sheetId:
            var sheetId = "1rl1vi-LUhOgoY_QrMJsm2UE0SdiL4EbOtLwfNPsavxQ";
            var sheetName = "UsageRules1"; // Has to match the sheet name
            var source = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);
            var store = source.GetTypeAdapter<UsageRule>();

            var analytics = CreateLocalAnalyticsSystem();
            IEnumerable<UsageRule> rules = await store.GetRulesInitialized(analytics);
            foreach (var rule in rules) {
                if (await rule.isTrue()) {
                    Log.d(JsonWriter.AsPrettyString(rule)); // TODO
                }
            }
            Assert.Single(rules.Filter(r => !r.andRules.IsNullOrEmpty()));

        }

        private static LocalAnalytics CreateLocalAnalyticsSystem() {
            LocalAnalytics analytics = new LocalAnalytics(new InMemoryKeyValueStore());
            analytics.createStoreFor = (_) => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>();
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

        async Task AssertFeatureUsageDetected(LocalAnalytics analytics, string featureId, int expectedCount) {
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

    }

}