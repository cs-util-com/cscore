using com.csutil.keyvaluestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.model {

    public class FeatureFlagManager<T> where T : IFeatureFlag {

        public static FeatureFlagManager<T> instance => IoC.inject.Get<FeatureFlagManager<T>>(null, false);

        private KeyValueStoreTypeAdapter<T> featureFlagStore;

        public FeatureFlagManager(KeyValueStoreTypeAdapter<T> featureFlagStore) {
            this.featureFlagStore = featureFlagStore;
        }

        public async Task<bool> IsFeatureEnabled(string featureId) {
            T flag = await GetFeatureFlag(featureId);
            if (flag == null) { return false; } // No feature returned so feature not enabled
            return await flag.IsEnabled();
        }

        public async Task<T> GetFeatureFlag(string featureId) {
            return await ReturnInitializedFlag(await featureFlagStore.Get(featureId, default(T)));
        }

        public async Task<IEnumerable<T>> GetAllFeatureFlags() {
            var all = await featureFlagStore.GetAll();
            return await all.MapAsync(async x => await ReturnInitializedFlag(x));
        }

        private async Task<T> ReturnInitializedFlag(T flag) {
            AssertV2.IsNotNull(flag.localState, "flag.localState");
            if (flag != null && flag.localState.randomPercentage == 0) {
                // if the server decided its a staged rollout no rnd % generated yet so do it:
                flag.localState.randomPercentage = new Random().Next(1, 100);
                await featureFlagStore.Set(flag.id, flag); // save in local store
            }
            return flag;
        }

    }

}