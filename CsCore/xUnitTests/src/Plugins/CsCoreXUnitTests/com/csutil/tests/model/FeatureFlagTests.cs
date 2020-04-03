using System.Threading.Tasks;
using com.csutil.keyvaluestore;
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
            IoC.inject.SetSingleton(new FeatureFlagManager(testStore));

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

            IoC.inject.SetSingleton(new FeatureFlagManager(testStore));

            var key1 = "MyFlag1";
            var key2 = "MyFlag2";
            var key3 = "MyFlag3";

            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key1, null));
            Assert.NotNull(await googleSheetsStore.Get<FeatureFlag>(key2, null));
            Assert.NotNull(await testStore.Get<FeatureFlag>(key2, null));
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
            Assert.True(flag3_2.IsEnabled());
            Assert.True(await FeatureFlag.IsEnabled(key3));

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