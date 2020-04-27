using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;

namespace com.csutil.model {

    public interface ProgressionSystem {

        Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag);
        Task<IEnumerable<FeatureFlag>> GetLockedFeatures();
        Task<IEnumerable<FeatureFlag>> GetUnlockedFeatures();

    }

    public class DefaultProgressionSystem : ProgressionSystem {

        public static async Task<DefaultProgressionSystem> SetupWithGSheets(string apiKey, string sheetId, string sheetName) {
            var cachedFlags = FileBasedKeyValueStore.New("FeatureFlags");
            var cachedFlagsLocalData = FileBasedKeyValueStore.New("FeatureFlags_LocalData");
            var googleSheetsStore = new GoogleSheetsKeyValueStore(cachedFlags, apiKey, sheetId, sheetName);
            return await Setup(new DefaultFeatureFlagStore(cachedFlagsLocalData, googleSheetsStore));
        }

        public static async Task<DefaultProgressionSystem> Setup(FeatureFlagStore featureFlagStore) {
            return await Setup(featureFlagStore, new LocalAnalytics());
        }

        public static async Task<DefaultProgressionSystem> Setup(FeatureFlagStore featureFlagStore, LocalAnalytics analytics) {
            var ffm = new FeatureFlagManager(featureFlagStore);
            IoC.inject.SetSingleton(ffm);
            AppFlow.AddAppFlowTracker(new AppFlowToStore(analytics));
            var xpSystem = new DefaultProgressionSystem(analytics, ffm);
            IoC.inject.SetSingleton<ProgressionSystem>(xpSystem);
            await xpSystem.UpdateCurrentCategoryCounts();
            return xpSystem;
        }

        public readonly Dictionary<string, float> xpFactors = InitXpFactors();
        public readonly Dictionary<string, int> cachedCategoryCounts = new Dictionary<string, int>();
        public readonly LocalAnalytics analytics;
        public readonly FeatureFlagManager featureFlagManager;

        public DefaultProgressionSystem(LocalAnalytics analytics, FeatureFlagManager featureFlagManager) {
            // Make sure the FeatureFlag system was set up too:
            AssertV2.NotNull(FeatureFlagManager.instance, "FeatureFlagManager.instance");
            this.analytics = analytics;
            this.featureFlagManager = featureFlagManager;
        }

        private static Dictionary<string, float> InitXpFactors() {
            var res = new Dictionary<string, float>();
            res.Add(EventConsts.catMutation, 1);
            res.Add(EventConsts.catView, 0.5f);
            res.Add(EventConsts.catUi, 0.1f);
            return res;
        }

        public async Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag) {
            await UpdateCurrentCategoryCounts();
            return LastCachedIsFeatureUnlocked(featureFlag);
        }

        private bool LastCachedIsFeatureUnlocked(FeatureFlag featureFlag) {
            return featureFlag.requiredXp <= GetLastCachedXp();
        }

        public async Task<IEnumerable<FeatureFlag>> GetLockedFeatures() {
            await UpdateCurrentCategoryCounts();
            var allFeatures = await featureFlagManager.GetAllFeatureFlags();
            return allFeatures.Filter(f => !LastCachedIsFeatureUnlocked(f));
        }

        public async Task<IEnumerable<FeatureFlag>> GetUnlockedFeatures() {
            await UpdateCurrentCategoryCounts();
            var allFeatures = await featureFlagManager.GetAllFeatureFlags();
            return allFeatures.Filter(f => LastCachedIsFeatureUnlocked(f));
        }

        public async Task UpdateCurrentCategoryCounts() {
            foreach (var xpFactor in xpFactors) {
                var category = xpFactor.Key;
                var store = analytics.GetStoreForCategory(category);
                var currentCountForCategory = (await store.GetAllKeys()).Count();
                cachedCategoryCounts.AddOrReplace(category, currentCountForCategory);
            }
        }

        public async Task<int> GetLatestXp() {
            await UpdateCurrentCategoryCounts();
            return GetLastCachedXp();
        }

        public int GetLastCachedXp() {
            float xp = 0;
            foreach (var f in xpFactors) {
                if (cachedCategoryCounts.TryGetValue(f.Key, out int count)) {
                    xp += count * f.Value;
                }
            }
            return (int)xp;
        }

        /// <summary> 
        /// Set a listener that is triggered whenever a new event is set on the main store of the LocalAnalytics,
        /// e.g. to update a progression UI 
        /// </summary>
        public void SetChangeListener(Func<string, object, object, Task> SetOnMainStore) {
            if (analytics.store is MutationObserverKeyValueStore l) {
                l.onSet = SetOnMainStore;
            }
        }

    }

}