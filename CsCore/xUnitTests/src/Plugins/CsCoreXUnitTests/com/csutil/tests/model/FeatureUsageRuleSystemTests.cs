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
                var featureUsedXDays = new FeatureDaysRule() { featureId = id, days = days };
                featureUsedXDays.isTrue = async () => await UsedXDays(featureUsedXDays, analytics);
                Assert.False(await featureUsedXDays.isTrue());
            }

            {
                var featureNotUsedXDays = new FeatureDaysRule() { featureId = id, days = days };
                featureNotUsedXDays.isTrue = async () => !await UsedXDays(featureNotUsedXDays, analytics);
                Assert.True(await featureNotUsedXDays.isTrue());
            }

            {
                var appNotUsedXDays = new AppDaysRule() { days = days };
                appNotUsedXDays.isTrue = async () => !await AppUsedXDays(appNotUsedXDays, analytics);
                Assert.True(await appNotUsedXDays.isTrue());
            }
            {
                var appUsedXDays = new AppDaysRule() { days = days };
                appUsedXDays.isTrue = async () => await AppUsedXDays(appUsedXDays, analytics);
                Assert.False(await appUsedXDays.isTrue());

                var featureUsedXTimes = new FeatureCounterRule() { featureId = id, timesUsed = times };
                featureUsedXTimes.isTrue = async () => await FeatureUsedXTimes(featureUsedXTimes, analytics);
                Assert.True(await featureUsedXTimes.isTrue());

                var featureNotUsedInTheLastXDays = new FeatureDaysRule() { featureId = id, days = days };
                featureNotUsedInTheLastXDays.isTrue = async () => !await FeatureUsedInTheLastXDays(featureNotUsedInTheLastXDays, analytics);
                Assert.False(await featureNotUsedInTheLastXDays.isTrue());

                var featureNotUsedAnymoreRule = new RuleConcat(appUsedXDays, featureUsedXTimes, featureNotUsedInTheLastXDays);
                Assert.False(await featureNotUsedAnymoreRule.isTrue());
            }

            {
                var featureNotUsedXTimes = new FeatureCounterRule() { featureId = id, timesUsed = times };
                featureNotUsedXTimes.isTrue = async () => !await FeatureUsedXTimes(featureNotUsedXTimes, analytics);
                Assert.False(await featureNotUsedXTimes.isTrue());
            }

            {
                var appUsedInTheLastXDays = new AppDaysRule() { days = days };
                appUsedInTheLastXDays.isTrue = () => Task.FromResult(AppUsedInTheLastXDays(appUsedInTheLastXDays));
                Assert.True(await appUsedInTheLastXDays.isTrue());
            }
            {
                var appNotUsedInTheLastXDays = new AppDaysRule() { days = days };
                appNotUsedInTheLastXDays.isTrue = () => Task.FromResult(!AppUsedInTheLastXDays(appNotUsedInTheLastXDays));
                Assert.False(await appNotUsedInTheLastXDays.isTrue());
            }

            {
                var featureUsedInTheLastXDays = new FeatureDaysRule() { featureId = id, days = days };
                featureUsedInTheLastXDays.isTrue = async () => await FeatureUsedInTheLastXDays(featureUsedInTheLastXDays, analytics);
                Assert.True(await featureUsedInTheLastXDays.isTrue());
            }

        }

        public static bool AppUsedInTheLastXDays(AppDaysRule self) {
            var tSinceLatestLaunch = DateTimeV2.UtcNow - EnvironmentV2.instance.systemInfo.GetLatestLaunchDate();
            return tSinceLatestLaunch.Days < self.days;
        }

        private static async Task<bool> FeatureUsedInTheLastXDays(FeatureDaysRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            DateTime lastEvent = (await featureEventStore.GetAll()).Last().GetDateTimeUtc();
            TimeSpan lastEventVsNow = DateTimeV2.UtcNow - lastEvent;
            return lastEventVsNow.Days <= self.days;
        }

        private static async Task<bool> FeatureUsedXTimes(FeatureCounterRule self, LocalAnalytics analytics) {
            var featureEventStore = analytics.categoryStores[self.featureId];
            var allFeatureEvents = await featureEventStore.GetAll();
            var startEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
            return startEvents.Count() >= self.timesUsed.Value;
        }

        private static async Task<bool> AppUsedXDays(AppDaysRule self, LocalAnalytics analytics) {
            var allAppEvents = await analytics.GetAll();
            var dayGroups = allAppEvents.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
        }

        private static async Task<bool> UsedXDays(FeatureDaysRule self, LocalAnalytics analytics) {
            var all = await analytics.categoryStores[self.featureId].GetAll();
            var dayGroups = all.GroupBy(x => x.GetDateTimeUtc().Date, x => x);
            return dayGroups.Count() >= self.days;
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