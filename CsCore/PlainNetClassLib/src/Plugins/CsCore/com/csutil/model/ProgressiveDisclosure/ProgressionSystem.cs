using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.logging.analytics;

namespace com.csutil.model {

    public class ProgressionSystem<T> : IProgressionSystem<T> where T : IFeatureFlag {

        public readonly Dictionary<string, float> xpFactors = InitDefaultXpFactors();
        public readonly Dictionary<string, int> cachedCategoryCounts = new Dictionary<string, int>();
        public readonly LocalAnalytics analytics;
        public readonly FeatureFlagManager<T> featureFlagManager;

        public ProgressionSystem(LocalAnalytics analytics, FeatureFlagManager<T> featureFlagManager) {
            // Make sure the FeatureFlag system was set up too:
            AssertV2.IsNotNull(FeatureFlagManager<T>.instance, "FeatureFlagManager.instance");
            this.analytics = analytics;
            this.featureFlagManager = featureFlagManager;
        }

        private static Dictionary<string, float> InitDefaultXpFactors() {
            var res = new Dictionary<string, float>();
            res.Add(EventConsts.catMutation, 1);
            res.Add(EventConsts.catView, 0.5f);
            res.Add(EventConsts.catUi, 0.1f);
            return res;
        }

        public async Task<bool> IsFeatureUnlocked(T featureFlag) {
            await UpdateCurrentCategoryCounts();
            return LastCachedIsFeatureUnlocked(featureFlag);
        }

        private bool LastCachedIsFeatureUnlocked(IFeatureFlag featureFlag) {
            return featureFlag.requiredXp <= GetLastCachedXp();
        }

        public async Task<IEnumerable<T>> GetLockedFeatures() {
            await UpdateCurrentCategoryCounts();
            var allFeatures = await featureFlagManager.GetAllFeatureFlags();
            return allFeatures.Filter(f => !LastCachedIsFeatureUnlocked(f));
        }

        public async Task<IEnumerable<T>> GetUnlockedFeatures() {
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
        [Obsolete("Use a store like the ObservableKeyValueStore (implements INotifyCollectionChanged) to listen to changes in the store")]
        public void SetChangeListener(Func<string, object, object, Task> SetOnMainStore) {
            if (analytics.store is MutationObserverKeyValueStore l) {
                l.onSet = SetOnMainStore;
            }
            if (analytics.store is INotifyCollectionChanged l2) {
                l2.CollectionChanged += ((sender, args) => {
                    var newItem = (KeyValuePair<string, object>)args.NewItems[0];
                    var oldItem = (KeyValuePair<string, object>?)(args.OldItems.Count > 0 ? args.OldItems[0] : null);
                    SetOnMainStore(newItem.Key, newItem.Value, oldItem?.Value);
                });
            }
        }

    }

}