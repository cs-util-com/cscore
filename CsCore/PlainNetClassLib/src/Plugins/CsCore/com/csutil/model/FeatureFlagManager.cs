using System;
using System.Threading.Tasks;
using com.csutil.json;

namespace com.csutil.model {

    public class FeatureFlagManager {

        public static FeatureFlagManager instance => IoC.inject.Get<FeatureFlagManager>(null, false);

        private IKeyValueStore store;
        private IJsonWriter typedJsonWriter = TypedJsonHelper.NewTypedJsonWriter();

        public FeatureFlagManager(IKeyValueStore featureFlagStore) { this.store = featureFlagStore; }

        public async Task<bool> isEnabled(string featureId) {
            FeatureFlag flag = await LoadFeatureFlag(featureId);
            if (flag == null || !flag.isEnabled) { return false; } // No feature returned so feature not enabled
            if (flag.isStagedRollout) { // if the server decided its a staged rollout..
                // if the client rnd value is below the rollout % this client gets the feature:
                return flag.localRandomValue <= flag.rolloutPercentServer;
            }
            return true; // The feature flag was returned so the feature is enabled
        }

        public async Task<FeatureFlag> LoadFeatureFlag(string featureId) {
            var key = GenerateFeatureKey(featureId);
            FeatureFlag flag = await store.Get<FeatureFlag>(key, null);
            if (flag != null && flag.isStagedRollout && flag.localRandomValue == 0) {
                // if the server decided its a staged rollout no rnd % generated yet so do it:
                flag.localRandomValue = new Random().Next(1, 100);
                await store.Set(key, flag); // save in local store
            }
            return flag;
        }

        private string GenerateFeatureKey(string featureId) {
            FeatureKey key = new FeatureKey() { id = featureId };
            return typedJsonWriter.Write(key);
        }

        public class FeatureKey {
            public EnvironmentV2.ISystemInfo sys = EnvironmentV2.instance.systemInfo;
            public string id;
        }

    }

    public class FeatureFlag {

        public static Task<bool> IsEnabled(string featureId) { return FeatureFlagManager.instance.isEnabled(featureId); }

        public bool isEnabled;
        public bool isStagedRollout;
        public int rolloutPercentServer; // between 0 and 100
        public int localRandomValue; // between 0 and 100

    }

}