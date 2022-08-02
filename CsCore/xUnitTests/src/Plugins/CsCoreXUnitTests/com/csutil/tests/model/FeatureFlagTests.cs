using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using com.csutil.model;
using Xunit;

namespace com.csutil.tests.model {

    [Collection("Sequential")] // Will execute tests in here sequentially
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
            var testStore = new FeatureFlagStore(new InMemoryKeyValueStore(), googleSheetsStore);
            IoC.inject.SetSingleton<FeatureFlagManager<FeatureFlag>>(new FeatureFlagManager<FeatureFlag>(testStore));

            // Open https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM for these:
            Assert.False(await FeatureFlag.IsEnabled("MyFlag1"));
            Assert.True(await FeatureFlag.IsEnabled("MyFlag2"));

            var key3 = "MyFlag3";
            IFeatureFlag flag3 = await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(key3);
            Assert.Equal(40, flag3.rolloutPercentage); // Rollout of feature 3 is at 40%

            // The local random value that was chosen determines if the flag is enabled or not:
            if (flag3.localState.randomPercentage <= flag3.rolloutPercentage) {
                Assert.True(await FeatureFlag.IsEnabled(key3));
            } else {
                Assert.False(await FeatureFlag.IsEnabled(key3));
            }

        }

        [Fact]
        public async Task ExampleUsage2() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM contains the sheetId:
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var xpSys = await DefaultProgressionSystem.SetupWithGSheets(apiKey, sheetId, sheetName);

            // The DefaultProgressionSystem will give 1 xp for each mutation:
            AppFlow.TrackEvent(EventConsts.catMutation, "Some mutation"); // Would also be triggered by DataStore
            Assert.NotEqual(0, await xpSys.GetLatestXp());
            Assert.True(await FeatureFlag.IsEnabled("MyFlag5")); // MyFlag5 needs minimum 1 xp

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
            var testStore = new FeatureFlagStore(localStore, googleSheetsStore);

            IoC.inject.SetSingleton<FeatureFlagManager<FeatureFlag>>(new FeatureFlagManager<FeatureFlag>(testStore));

            var key1 = "MyFlag1";
            var key2 = "MyFlag2";
            var key3 = "MyFlag3";

            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key1, null));
            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key2, null));
            Assert.NotNull(await testStore.Get(key2, null));
            Assert.NotNull(await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(key2));

            Assert.False(await FeatureFlag.IsEnabled(key1));
            Assert.True(await FeatureFlag.IsEnabled(key2));

            var flag3_1 = await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(key3);
            Assert.Equal(40, flag3_1.rolloutPercentage);

            var localState3_1 = await localStore.Get<IFeatureFlagLocalState>(key3, null);
            var percent3_1 = localState3_1.randomPercentage;
            Assert.NotEqual(0, percent3_1);
            if (percent3_1 < flag3_1.rolloutPercentage) {
                Assert.True(await FeatureFlag.IsEnabled(key3));
            } else {
                Assert.False(await FeatureFlag.IsEnabled(key3));
            }
            localState3_1.randomPercentage = flag3_1.rolloutPercentage - 1;
            await localStore.Set(key3, localState3_1);

            var localState3_2 = await localStore.Get<IFeatureFlagLocalState>(key3, null);
            Assert.NotEqual(percent3_1, localState3_2.randomPercentage);

            var flag3_2 = await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(key3);
            Assert.Equal(localState3_2.randomPercentage, flag3_2.localState.randomPercentage);
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
            var testStore = new FeatureFlagStore(new InMemoryKeyValueStore(), googleSheetsStore);
            IoC.inject.SetSingleton<FeatureFlagManager<FeatureFlag>>(new FeatureFlagManager<FeatureFlag>(testStore));

            // Make sure user would normally be included in the rollout:
            var flagId4 = "MyFlag4";
            var flag4 = await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(flagId4);
            Assert.Equal(flagId4, flag4.id);
            Assert.Equal(100, flag4.rolloutPercentage);

            // There is no user progression system setup so the requiredXp value of the feature flag is ignored:
            Assert.True(await FeatureFlag.IsEnabled(flagId4));
            Assert.True(await flag4.IsFeatureUnlocked());

            // Setup progression system and check again:
            var xpSystem = new TestXpSystem();
            IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);
            // Now that there is a progression system 
            Assert.False(await flag4.IsFeatureUnlocked());
            Assert.False(await FeatureFlag.IsEnabled(flagId4));

            var eventCount = 1000;

            var store = new ObservableKeyValueStore(new InMemoryKeyValueStore());
            // Lets assume the users xp correlates with the number of triggered local analytics events:
            store.CollectionChanged += delegate {
                xpSystem.currentXp++;
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

        private class TestXpSystem : IProgressionSystem<FeatureFlag> {

            public int currentXp = 0;

            public Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag) {
                return Task.FromResult(featureFlag.requiredXp <= currentXp);
            }

            public Task<IEnumerable<FeatureFlag>> GetLockedFeatures() { throw new NotImplementedException(); }

            public Task<IEnumerable<FeatureFlag>> GetUnlockedFeatures() { throw new NotImplementedException(); }

        }

        [Fact]
        public async Task TestDefaultProgressionSystem() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM contains the sheetId:
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            var googleSheetsStore = new GoogleSheetsKeyValueStore(new InMemoryKeyValueStore(), apiKey, sheetId, sheetName);
            var ffm = new FeatureFlagManager<FeatureFlag>(new FeatureFlagStore(new InMemoryKeyValueStore(), googleSheetsStore));
            IoC.inject.SetSingleton<FeatureFlagManager<FeatureFlag>>(ffm);

            LocalAnalytics analytics = new LocalAnalytics();
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));
            var xpSystem = new ProgressionSystem<FeatureFlag>(analytics, ffm);
            IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);

            await CleanupFilesFromTest(analytics, xpSystem);

            // Simulate User progression by causing analytics events:
            var eventCount = 1000;
            for (int i = 0; i < eventCount; i++) {
                AppFlow.TrackEvent(EventConsts.catMutation, "User did mutation nr " + i);
            }

            var flagId4 = "MyFlag4";
            var flag4 = await FeatureFlagManager<FeatureFlag>.instance.GetFeatureFlag(flagId4);
            Assert.Equal(1000, flag4.requiredXp); // The user needs >= 1000 XP for the feature

            // Now that the user has 1000 XP the condition of the TestXpSystem is met:
            Assert.True(await FeatureFlag.IsEnabled(flagId4));

            // The number of mutation events:
            Assert.Equal(eventCount, xpSystem.cachedCategoryCounts[EventConsts.catMutation]);
            // Since there are only mutation events the XP is equal to the factor*event count:
            Assert.Equal(await xpSystem.GetLatestXp(), eventCount * xpSystem.xpFactors[EventConsts.catMutation]);

            await CleanupFilesFromTest(analytics, xpSystem);
        }

        // Cleanup from previous tests (needed because persistance to disc is used):
        private static async Task CleanupFilesFromTest(LocalAnalytics analytics, ProgressionSystem<FeatureFlag> xpSystem) {
            // First trigger 1 event for each relevant catory to load the category stores:
            foreach (var category in xpSystem.xpFactors.Keys) { AppFlow.TrackEvent(category, "Dummy Event"); }
            await analytics.RemoveAll(); // Then clear all stores
            Assert.Empty(await analytics.GetAllKeys()); // Now the main store should be emtpy
        }

        [Fact]
        public async Task ExtensiveDefaultProgressionSystemTests() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // https://docs.google.com/spreadsheets/d/1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM contains the sheetId:
            var sheetId = "1KBamVmgEUX-fyogMJ48TT6h2kAMKyWU1uBL5skCGRBM";
            var sheetName = "MySheet1"; // Has to match the sheet name
            ProgressionSystem<FeatureFlag> xpSys = await NewInMemoryTestXpSystem(apiKey, sheetId, sheetName);

            Assert.Empty(await xpSys.analytics.GetStoreForCategory(EventConsts.catMutation).GetAllKeys());
            Assert.Equal(0, await xpSys.GetLatestXp());

            Assert.False(await FeatureFlag.IsEnabled("MyFlag5")); // Needs 1 xp

            // The DefaultProgressionSystem will give 1 xp for each mutation:
            AppFlow.TrackEvent(EventConsts.catMutation, "Some mutation"); // Would also be triggered by DataStore
            Assert.Single(await xpSys.analytics.GetStoreForCategory(EventConsts.catMutation).GetAllKeys());
            Assert.NotEqual(0, await xpSys.GetLatestXp());

            Assert.False(await FeatureFlag.IsEnabled("MyFlag4")); // Needs 1000 xp
            Assert.True(await FeatureFlag.IsEnabled("MyFlag5")); // Needs 1 xp

            await GetLockedAndUnlockedFeatures(xpSys, xpSys.GetLastCachedXp());

        }

        private static async Task GetLockedAndUnlockedFeatures(IProgressionSystem<FeatureFlag> xpSys, int currentUserXp) {
            var t = Log.MethodEntered();
            {
                var lockedFeatures = await xpSys.GetLockedFeatures();
                var unlockedFeatures = await xpSys.GetUnlockedFeatures();
                Assert.NotEmpty(lockedFeatures);
                Assert.NotEmpty(unlockedFeatures);
                Assert.Empty(lockedFeatures.Intersect(unlockedFeatures)); // There should be no features in both lists

                foreach (var feature in lockedFeatures) {
                    Assert.True(currentUserXp < feature.requiredXp);
                    Assert.False(await xpSys.IsFeatureUnlocked(feature));
                }
                foreach (var feature in unlockedFeatures) {
                    Assert.True(currentUserXp >= feature.requiredXp);
                    Assert.True(await xpSys.IsFeatureUnlocked(feature));
                }

                var nextAvailableFeature = lockedFeatures.OrderBy(f => f.requiredXp).First();
                Log.d("The next unlocked feature will be " + JsonWriter.AsPrettyString(nextAvailableFeature));

            }
            Log.MethodDone(t);
        }

        private static async Task<ProgressionSystem<FeatureFlag>> NewInMemoryTestXpSystem(string apiKey, string sheetId, string sheetName) {
            var cachedFlags = new InMemoryKeyValueStore();
            var googleSheetsStore = new GoogleSheetsKeyValueStore(cachedFlags, apiKey, sheetId, sheetName);
            var cachedFlagsLocalData = new InMemoryKeyValueStore();
            var analytics = new LocalAnalytics(new InMemoryKeyValueStore());
            analytics.createStoreFor = (_ => new InMemoryKeyValueStore().GetTypeAdapter<AppFlowEvent>());
            var featureFlagStore = new FeatureFlagStore(cachedFlagsLocalData, googleSheetsStore);
            return await DefaultProgressionSystem.Setup(featureFlagStore, analytics);
        }

    }

}