using System;
using System.Collections.Generic;
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

        public FeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore)
                                                        : base(localStore, remoteStore) { }

        // By default just return the featureId:
        protected override string GenerateFeatureKey(string featureId) { return featureId; }

    }

    public static class DefaultProgressionSystem {

        public static async Task<ProgressionSystem<FeatureFlag>> SetupWithGSheets(string apiKey, string sheetId, string sheetName, HashSet<Tuple<object, Type>> collectedInjectors = null) {
            var cachedFlags = FileBasedKeyValueStore.New("FeatureFlags");
            var cachedFlagsLocalData = FileBasedKeyValueStore.New("FeatureFlags_LocalData");
            var googleSheetsStore = new GoogleSheetsKeyValueStore(cachedFlags, apiKey, sheetId, sheetName);
            return await Setup(new FeatureFlagStore(cachedFlagsLocalData, googleSheetsStore), collectedInjectors);
        }

        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, HashSet<Tuple<object, Type>> collectedInjectors = null) {
            return await Setup(featureFlagStore, new LocalAnalytics(), collectedInjectors);
        }

        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, LocalAnalytics analytics, HashSet<Tuple<object, Type>> collectedInjectors = null) {
            var ffm = new FeatureFlagManager<FeatureFlag>(featureFlagStore);
            var injector1 = IoC.inject.SetSingleton(ffm);
            collectedInjectors?.Add(new Tuple<object, Type>(injector1, typeof(FeatureFlagManager<FeatureFlag>)));
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics).WithBasicTrackingActive());
            var xpSystem = new ProgressionSystem<FeatureFlag>(analytics, ffm);
            var injector2 = IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);
            collectedInjectors?.Add(new Tuple<object, Type>(injector2, typeof(IProgressionSystem<FeatureFlag>)));
            await xpSystem.UpdateCurrentCategoryCounts();
            return xpSystem;
        }

    }

}