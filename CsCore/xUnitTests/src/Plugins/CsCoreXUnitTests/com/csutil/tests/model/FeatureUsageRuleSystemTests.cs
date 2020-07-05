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

            string featureId = "feature1";
            var days = 20;
            var times = 10;

            var analytics = SetupLocalAnalyticsSystem();
            await SimulateFeatureUsage(featureId, eventCount: 100);
            {
                var featureEventStore = analytics.categoryStores[featureId];
                var allFeatureEvents = await featureEventStore.GetAll();
                Assert.Equal(100, allFeatureEvents.Count());
                var allStartEvents = allFeatureEvents.Filter(x => x.action == EventConsts.START);
                Assert.Equal(100, allStartEvents.Count());
            }
            {
                UsageRule rule = analytics.NewFeatureUsedXDaysRule(featureId, days);
                Assert.False(await rule.isTrue());
            }
            {
                UsageRule rule = analytics.NewFeatureNotUsedXDaysRule(featureId, days);
                Assert.True(await rule.isTrue());
            }
            {
                UsageRule rule = analytics.NewAppNotUsedXDaysRule(days);
                Assert.True(await rule.isTrue());
            }
            {
                UsageRule rule1 = analytics.NewAppUsedXDaysRule(days);
                Assert.False(await rule1.isTrue());

                UsageRule rule2 = analytics.NewFeatureUsedXTimesRule(featureId, times);
                Assert.True(await rule2.isTrue());

                UsageRule rule3 = analytics.NewFeatureNotUsedInTheLastXDaysRule(featureId, days);
                Assert.False(await rule3.isTrue());

                UsageRule featureNotUsedAnymoreRule = analytics.NewConcatRule(rule1, rule2, rule3);
                Assert.False(await featureNotUsedAnymoreRule.isTrue());
            }
            {
                UsageRule rule = analytics.NewFeatureNotUsedXTimesRule(featureId, times);
                Assert.False(await rule.isTrue());
            }
            {
                UsageRule rule = analytics.NewAppUsedInTheLastXDaysRule(days);
                Assert.True(await rule.isTrue());
            }
            {
                UsageRule rule = analytics.NewAppNotUsedInTheLastXDaysRule(days);
                Assert.False(await rule.isTrue());
            }
            {
                UsageRule rule = analytics.NewFeatureUsedInTheLastXDaysRule(featureId, days);
                Assert.True(await rule.isTrue());
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