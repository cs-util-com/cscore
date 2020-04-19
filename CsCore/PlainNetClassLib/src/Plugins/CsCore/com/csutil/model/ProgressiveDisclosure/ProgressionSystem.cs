using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;

namespace com.csutil.model {

    public interface ProgressionSystem {
        Task<bool> IsFeatureUnlocked(FeatureFlag featureFlag);
    }

    public class DefaultProgressionSystem : ProgressionSystem {

        public readonly Dictionary<string, float> xpFactors = InitXpFactors();
        public readonly Dictionary<string, int> cachedCategoryCounts = new Dictionary<string, int>();

        private LocalAnalytics analytics;

        public DefaultProgressionSystem(LocalAnalytics analytics) {
            // Make sure the FeatureFlag system was set up too:
            AssertV2.NotNull(FeatureFlagManager.instance, "FeatureFlagManager.instance");
            this.analytics = analytics;
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
            return featureFlag.requiredXp <= GetCurrentXp();
        }

        public async Task UpdateCurrentCategoryCounts() {
            foreach (var xpFactor in xpFactors) {
                var category = xpFactor.Key;
                if (analytics.categoryStores.TryGetValue(category, out KeyValueStoreTypeAdapter<AppFlowEvent> store)) {
                    var currentCountForCategory = (await store.GetAllKeys()).Count();
                    cachedCategoryCounts.AddOrReplace(category, currentCountForCategory);
                }
            }
        }

        public int GetCurrentXp() {
            float xp = 0;
            foreach (var f in xpFactors) {
                if (cachedCategoryCounts.TryGetValue(f.Key, out int count)) { xp += count * f.Value; }
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