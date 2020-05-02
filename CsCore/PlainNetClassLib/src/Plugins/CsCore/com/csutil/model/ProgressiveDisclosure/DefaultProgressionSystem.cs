using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;

namespace com.csutil.model {

    /// <summary> 
    /// The default implementation of IFeatureFlag that for most usecases should 
    /// be enough. See the FeatureFlagTests for usage examples.
    /// </summary>
    public class FeatureFlag : IFeatureFlag {

        public static Task<bool> IsEnabled(string featureId) {
            return FeatureFlagManager<FeatureFlag>.instance.IsFeatureEnabled(featureId);
        }

        public string id { get; set; }
        public int rolloutPercentage { get; set; }
        public int requiredXp { get; set; }
        public IFeatureFlagLocalState localState { get; set; } = new FeatureFlagLocalState();

    }

    public class FeatureFlagLocalState : IFeatureFlagLocalState {
        public int randomPercentage { get; set; }
    }

    public class FeatureFlagStore : BaseFeatureFlagStore<FeatureFlag, FeatureFlagLocalState> {

        public FeatureFlagStore(IKeyValueStore l, IKeyValueStore r) : base(l, r) { }

        // By default just return the featureId:
        protected override string GenerateFeatureKey(string featureId) { return featureId; }

    }

    public static class DefaultProgressionSystem {

        public static async Task<ProgressionSystem<FeatureFlag>> SetupWithGSheets(string apiKey, string sheetId, string sheetName) {
            var cachedFlags = FileBasedKeyValueStore.New("FeatureFlags");
            var cachedFlagsLocalData = FileBasedKeyValueStore.New("FeatureFlags_LocalData");
            var googleSheetsStore = new GoogleSheetsKeyValueStore(cachedFlags, apiKey, sheetId, sheetName);
            return await Setup(new FeatureFlagStore(cachedFlagsLocalData, googleSheetsStore));
        }

        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore) {
            return await Setup(featureFlagStore, new LocalAnalytics());
        }

        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, LocalAnalytics analytics) {
            var ffm = new FeatureFlagManager<FeatureFlag>(featureFlagStore);
            IoC.inject.SetSingleton(ffm);
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics).WithBasicTrackingActive());
            var xpSystem = new ProgressionSystem<FeatureFlag>(analytics, ffm);
            IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);
            await xpSystem.UpdateCurrentCategoryCounts();
            return xpSystem;
        }

    }

}