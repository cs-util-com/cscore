using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model;
using com.csutil.model.usagerules;
using Xunit;

namespace com.csutil.tests.model {

    public class FeatureUsageRuleSystemTests {

        public FeatureUsageRuleSystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            string id = "feature1";
            var days = 20;
            var times = 10;

            var analytics = SetupLocalAnalyticsSystem();
            await SimulateFeatureUsage(id, eventCount: 100);
            {
                var featureEventStore = analytics.categoryStores[id];
                var allFeatureEvents = await featureEventStore.GetAll();
                Assert.Equal(100, allFeatureEvents.Count());
                var allStartEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
                Assert.Equal(100, allStartEvents.Count());
            }

            {
                var featureUsedXDays = new FeatureUsedXDays() { featureId = id, days = days };
                featureUsedXDays.isTrue = async () => {
                    var all = await analytics.categoryStores[featureUsedXDays.featureId].GetAll();
                    var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
                    return dayGroups.Count() >= featureUsedXDays.days;
                };
                Assert.False(await featureUsedXDays.isTrue());
            }

            {
                var featureNotUsedXDays = new FeatureNotUsedXDays() { featureId = id, days = days };
                featureNotUsedXDays.isTrue = async () => {
                    var all = await analytics.categoryStores[featureNotUsedXDays.featureId].GetAll();
                    var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
                    return dayGroups.Count() < featureNotUsedXDays.days;
                };
                Assert.True(await featureNotUsedXDays.isTrue());
            }

            {
                var appNotUsedXDays = new AppNotUsedXDays() { days = days };
                appNotUsedXDays.isTrue = async () => {
                    var all = await analytics.GetAll();
                    var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
                    return dayGroups.Count() < appNotUsedXDays.days;
                };
                Assert.True(await appNotUsedXDays.isTrue());
            }

            {
                var appUsedXDays = new AppUsedXDays() { days = days };
                appUsedXDays.isTrue = async () => {
                    var all = await analytics.GetAll();
                    var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
                    return dayGroups.Count() >= appUsedXDays.days;
                };
                Assert.False(await appUsedXDays.isTrue());

                var featureUsedXTimes = new FeatureUsedXTimes() { featureId = id, timesUsed = times };
                featureUsedXTimes.isTrue = async () => {
                    var featureEventStore = analytics.categoryStores[featureUsedXTimes.featureId];
                    var allFeatureEvents = await featureEventStore.GetAll();
                    var startEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
                    Log.d("startEvents.Count=" + startEvents.Count());
                    return startEvents.Count() >= featureUsedXTimes.timesUsed.Value;
                };
                Assert.True(await featureUsedXTimes.isTrue());

                var featureNotUsedInTheLastXDays = new FeatureNotUsedInTheLastXDays() { featureId = id, days = days };
                featureNotUsedInTheLastXDays.isTrue = async () => {
                    var featureEventStore = analytics.categoryStores[featureUsedXTimes.featureId];
                    DateTime lastEvent = (await featureEventStore.GetAll()).Last().GetDateTimeUtc();
                    TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
                    return lastEventVsNow.Days > featureNotUsedInTheLastXDays.days;
                };
                Assert.False(await featureNotUsedInTheLastXDays.isTrue());

                var featureNotUsedAnymoreRule = new RuleConcat(appUsedXDays, featureUsedXTimes, featureNotUsedInTheLastXDays);
                Assert.False(await featureNotUsedAnymoreRule.isTrue());
            }

            {
                var featureNotUsedXTimes = new FeatureNotUsedXTimes() { featureId = id, timesUsed = times };
                featureNotUsedXTimes.isTrue = async () => {
                    var featureEventStore = analytics.categoryStores[featureNotUsedXTimes.featureId];
                    var allFeatureEvents = await featureEventStore.GetAll();
                    allFeatureEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
                    return allFeatureEvents.Count() < featureNotUsedXTimes.timesUsed.Value;
                };
                Assert.False(await featureNotUsedXTimes.isTrue());
            }

            {
                var appUsedInTheLastXDays = new AppUsedInTheLastXDays() { days = days };
                Assert.True(await appUsedInTheLastXDays.isTrue());
            }
            {
                var appNotUsedInTheLastXDays = new AppNotUsedInTheLastXDays() { days = days };
                Assert.False(await appNotUsedInTheLastXDays.isTrue());
            }

            {
                var featureUsedInTheLastXDays = new FeatureUsedInTheLastXDays() { featureId = id, days = days };
                featureUsedInTheLastXDays.isTrue = async () => {
                    var featureEventStore = analytics.categoryStores[featureUsedInTheLastXDays.featureId];
                    DateTime lastEvent = (await featureEventStore.GetAll()).Last().GetDateTimeUtc();
                    TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
                    return lastEventVsNow.Days < featureUsedInTheLastXDays.days;
                };
                Assert.True(await featureUsedInTheLastXDays.isTrue());
            }

        }

        private static LocalAnalytics SetupLocalAnalyticsSystem() {
            // Set the store to be the target of the local analytics so that whenever any 
            LocalAnalytics analytics = new LocalAnalytics(new InMemoryKeyValueStore());
            analytics.createStoreFor = (_) => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>();
            // Setup the AppFlow logic to use LocalAnalytics:
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));
            return analytics;
        }

        private static async Task SimulateFeatureUsage(string featureId, int eventCount) {
            // Simulate User progression by causing analytics events:
            for (int i = 0; i < eventCount; i++) {
                AppFlow.TrackEvent(featureId, EventConsts.START);
                await TaskV2.Delay(1);
            }
        }

        [Fact]
        public async Task ExampleUsage2() {

            var testStore = new TestFeatureStore();
            var mngr = new FeatureFlagManager<TestFeature>(testStore);
            var f1 = await mngr.GetFeatureFlag("f1");
            Assert.Null(f1);

            // var xpSystem = new TestXpSystem();

        }

        // private class TestXpSystem : IProgressionSystem<TestFeature> {
        //     public Task<IEnumerable<TestFeature>> GetLockedFeatures() { throw new System.NotImplementedException(); }
        //     public Task<IEnumerable<TestFeature>> GetUnlockedFeatures() { throw new System.NotImplementedException(); }
        //     public Task<bool> IsFeatureUnlocked(TestFeature featureFlag) { throw new System.NotImplementedException(); }
        // }


        private class TestFeatureStore : BaseFeatureFlagStore<TestFeature, IFeatureFlagLocalState> {
            public TestFeatureStore() : base(new InMemoryKeyValueStore(), new InMemoryKeyValueStore()) { }
            protected override string GenerateFeatureKey(string featureId) { return featureId; }
        }

        private class TestFeature : IFeatureFlag {
            public string id { get; set; }
            public int rolloutPercentage { get; set; }
            public int requiredXp { get; set; }
            public IFeatureFlagLocalState localState { get; set; } = new FeatureFlagLocalState();
        }

    }

}