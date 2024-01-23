using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;
using Zio;

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

        public FeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) : base(localStore, remoteStore) {
        }

        // By default just return the featureId:
        protected override string GenerateFeatureKey(string featureId) { return featureId; }

    }

    public static class DefaultProgressionSystem {

        [Obsolete("Consider using SetupWithGSheetsV2 instead")]
        public static async Task<ProgressionSystem<FeatureFlag>> SetupWithGSheets(string apiKey, string sheetId, string sheetName, IDisposableCollection disposables = null) {
            var cachedFlags = FileBasedKeyValueStore.New("FeatureFlags");
            disposables?.Add(cachedFlags);
            var cachedFlagsLocalData = FileBasedKeyValueStore.New("FeatureFlags_LocalData");
            disposables?.Add(cachedFlagsLocalData);
            var googleSheetsStore = new GoogleSheetsKeyValueStore(cachedFlags, apiKey, sheetId, sheetName);
            disposables?.Add(googleSheetsStore);
            return await Setup(new FeatureFlagStore(cachedFlagsLocalData, googleSheetsStore), disposables);
        }

        public static async Task<ProgressionSystem<FeatureFlag>> SetupWithGSheetsV2(Uri gSheetsUri, DirectoryEntry localAnalyticsFolder, IKeyValueStore cachedFlagsLocalData, IKeyValueStore gSheetsChache, IDisposableCollection disposables = null) {
            var googleSheetsStore = new GoogleSheetsKeyValueStoreV2(gSheetsChache, gSheetsUri);
            disposables?.Add(googleSheetsStore);
            return await SetupV2(new FeatureFlagStore(cachedFlagsLocalData, googleSheetsStore), localAnalyticsFolder, disposables);
        }

        [Obsolete("Use SetupV2 instead")]
        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, IDisposableCollection disposables = null) {
            var localAnalytics = new LocalAnalytics();
            disposables?.Add(localAnalytics);
            return await Setup(featureFlagStore, localAnalytics, disposables);
        }

        public static async Task<ProgressionSystem<FeatureFlag>> SetupV2(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, DirectoryEntry localAnalyticsFolder, IDisposableCollection disposables = null) {
            var localAnalyticsV3 = new LocalAnalyticsV3(localAnalyticsFolder);
            disposables?.Add(localAnalyticsV3);
            return await SetupV2(featureFlagStore, localAnalyticsV3, disposables);
        }

        [Obsolete("Use SetupV2 instead")]
        public static async Task<ProgressionSystem<FeatureFlag>> Setup(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, LocalAnalytics analytics, IDisposableCollection disposables = null) {
            var ffm = new FeatureFlagManager<FeatureFlag>(featureFlagStore);
            var injector1 = IoC.inject.SetSingleton(ffm);
            disposables?.Add(ffm);
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics).WithBasicTrackingActive());
            var xpSystem = new ProgressionSystem<FeatureFlag>(analytics, ffm);
            var injector2 = IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);
            disposables?.Add(xpSystem);
            await xpSystem.UpdateCurrentCategoryCounts();
            return xpSystem;
        }

        public static async Task<ProgressionSystem<FeatureFlag>> SetupV2(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore, LocalAnalyticsV3 analytics, IDisposableCollection disposables = null) {
            var ffm = new FeatureFlagManager<FeatureFlag>(featureFlagStore);
            var injector1 = IoC.inject.SetSingleton(ffm);
            disposables?.Add(ffm);
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics).WithBasicTrackingActive());
            var xpSystem = new ProgressionSystem<FeatureFlag>(analytics, ffm);
            var injector2 = IoC.inject.SetSingleton<IProgressionSystem<FeatureFlag>>(xpSystem);
            disposables?.Add(xpSystem);
            await xpSystem.UpdateCurrentCategoryCounts();
            return xpSystem;
        }

    }

}