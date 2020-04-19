using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model;
using Xunit;

namespace com.csutil.tests.model {

    public class FeatureFlagTests {

        public FeatureFlagTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM contains the sheetId:
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var googleSheetsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);
            var testStore = new TestFeatureFlagStore(new InMemoryKeyValueStore(), googleSheetsStore);
            IoC.inject.SetSingleton<FeatureFlagManager>(new FeatureFlagManager(testStore));

            // Open https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM for these:
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));
            Assert.True(await FeatureFlag.IsEnabled("MyFlag2"));

            var key3 = "MyFlag3";
            FeatureFlag flag3 = await FeatureFlagManager.instance.GetFeatureFlag(key3);
            Assert.Equal(40, flag3.rolloutPercentage); // Rollout of feature 3 is at 40%

            // The local random value that was chosen determines if the flag is enabled or not:
            if (flag3.localState.randomPercentage <= flag3.rolloutPercentage) {
                Assert.True(await FeatureFlag.IsEnabled(key3));
            } else {
                Assert.False(await FeatureFlag.IsEnabled(key3));
            }

        }

        [Fact]
        public async Task ExtendedTest1() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var googleSheetsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);


            var localStore = new InMemoryKeyValueStore();
            var testStore = new TestFeatureFlagStore(localStore, googleSheetsStore);

            IoC.inject.SetSingleton<FeatureFlagManager>(new FeatureFlagManager(testStore));

            var key1 = "MyFlag1";
            var key2 = "MyFlag2";
            var key3 = "MyFlag3";

            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key1, null));
            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key2, null));
            Assert.NotNull(await testStore.Get(key2, null));
            Assert.NotNull(await FeatureFlagManager.instance.GetFeatureFlag(key2));

            Assert.False(await FeatureFlag.IsEnabled(key1));
            Assert.True(await FeatureFlag.IsEnabled(key2));

            var flag3_1 = await FeatureFlagManager.instance.GetFeatureFlag(key3);
            Assert.Equal(40, flag3_1.rolloutPercentage);

            var state3_1 = await localStore.Get<FeatureFlag.LocalState>(key3, null);
            var percent3_1 = state3_1.randomPercentage;
            Assert.NotEqual(0, percent3_1);
            if (percent3_1 < flag3_1.rolloutPercentage) {
                Assert.True(await FeatureFlag.IsEnabled(key3));
            } else {
                Assert.False(await FeatureFlag.IsEnabled(key3));
            }
            state3_1.randomPercentage = flag3_1.rolloutPercentage - 1;
            await localStore.Set(key3, state3_1);

            var state3_2 = await localStore.Get<FeatureFlag.LocalState>(key3, null);
            Assert.NotEqual(percent3_1, state3_2.randomPercentage);

            var flag3_2 = await FeatureFlagManager.instance.GetFeatureFlag(key3);
            Assert.Equal(state3_2.randomPercentage, flag3_2.localState.randomPercentage);
            Assert.True(await flag3_2.IsEnabled());
            Assert.True(await FeatureFlag.IsEnabled(key3));

        }

        [Fact]
        public async Task TestProgressiveDisclosure() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM contains the sheetId:
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var googleSheetsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);
            var testStore = new TestFeatureFlagStore(new InMemoryKeyValueStore(), googleSheetsStore);
            IoC.inject.SetSingleton<FeatureFlagManager>(new FeatureFlagManager(testStore));

            // Make sure user would normally be included in the rollout:
            var flagId4 = "MyFlag4";
            var flag4 = await FeatureFlagManager.instance.GetFeatureFlag(flagId4);
            Assert.Equal(flagId4, flag4.id);
            Assert.Equal(100, flag4.rolloutPercentage);

            // There is no user progression system setup so the requiredXp value of the feature flag is ignored:
            Assert.True(await FeatureFlag.IsEnabled(flagId4));
            Assert.True(await flag4.IsFeatureUnlocked());

            // Setup progression system and check again:
            var xpSystem = new TestXpSystem();
            IoC.inject.SetSingleton<ProgressionSystem>(xpSystem);
            // Now that there is a progression system 
            Assert.False(await FeatureFlag.IsEnabled(flagId4));
            Assert.False(await flag4.IsFeatureUnlocked());

            var eventCount = 1000;

            var store = new MutationObserverKeyValueStore().WithFallbackStore(new InMemoryKeyValueStore());
            // Lets assume the users xp correlates with the number of triggered local analytics events:
            store.onSet = delegate {
                xpSystem.currentXp++;
                return Task.FromResult(true);
            };

            // Set the store to be the target of the local analytics so that whenever any 
            LocalAnalytics analytics = new LocalAnalytics(store);
            analytics.createStoreFor = (_) => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>();

            // Setup the AppFlow logic to use LocalAnalytics:
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));

            // Simulate User progression by causing analytics events:
            for (int i = 0; i < eventCount; i++) {
                AppFlow.TrackEvent(EventConsts.catMutation, "User did mutation nr " + i);
            }

            // Get the analtics store for category "Mutations":
            var mutationStore = await analytics.GetStoreForCategory(EventConsts.catMutation).GetAll();
            Assert.Equal(eventCount, mutationStore.Count()); // All events so far were mutations
            Assert.True(eventCount <= xpSystem.currentXp); // The user received xp for each mutation

            Assert.Equal(1000, flag4.requiredXp); // The user needs >= 1000 xp for the feature

            // Now that the user has more than 1000 xp the condition of the TestXpSystem is met:
            Assert.True(await flag4.IsFeatureUnlocked());
            Assert.True(await FeatureFlag.IsEnabled(flagId4));
        }

        private class TestXpSystem : ProgressionSystem {

            public int currentXp = 0;

            public Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag) {
                return Task.FromResult(featureFlag.requiredXp <= currentXp);
            }

        }

        private class TestFeatureFlagStore : DefaultFeatureFlagStore {

            public TestFeatureFlagStore(IKeyValueStore l, IKeyValueStore r) : base(l, r) { }

            protected override string GenerateFeatureKey(string featureId) {
                // Here e.g the EnvironmentV2.instance.systemInfo.osPlatform could be added to
                // allow different rollout percentages for different target platforms 
                return featureId; // for the test just return the featureId
            }

        }

    }

}