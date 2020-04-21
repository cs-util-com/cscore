using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;

namespace com.csutil.model {

    public class FeatureFlagManager {

        public static FeatureFlagManager instance => IoC.inject.Get<FeatureFlagManager>(null, false);

        private KeyValueStoreTypeAdapter<FeatureFlag> store;

        public FeatureFlagManager(KeyValueStoreTypeAdapter<FeatureFlag> featureFlagStore) {
            store = featureFlagStore;
        }

        public async Task<bool> IsEnabled(string featureId) {
            FeatureFlag flag = await GetFeatureFlag(featureId);
            if (flag == null) { return false; } // No feature returned so feature not enabled
            return await flag.IsEnabled();
        }

        public async Task<FeatureFlag> GetFeatureFlag(string featureId) {
            FeatureFlag flag = await store.Get(featureId, null);
            if (flag != null && flag.localState.randomPercentage == 0) {
                // if the server decided its a staged rollout no rnd % generated yet so do it:
                flag.localState.randomPercentage = new Random().Next(1, 100);
                await store.Set(flag.id, flag); // save in local store
            }
            return flag;
        }

    }

}