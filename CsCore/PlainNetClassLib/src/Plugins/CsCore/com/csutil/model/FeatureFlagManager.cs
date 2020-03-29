using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.model {

    public class FeatureFlag {

        public static Task<bool> IsEnabled(string featureId) { return FeatureFlagManager.instance.IsEnabled(featureId); }

        public int rolloutPercentage;

        public LocalState localState = new LocalState();

        // if the client rnd value is below the rollout % this client gets the feature:
        public bool IsEnabled() {
            AssertV2.AreNotEqual(0, localState.randomPercentage, "localState.randomPercentage");
            return localState.randomPercentage <= rolloutPercentage;
        }

        public class LocalState { public int randomPercentage; }

    }

    public class FeatureFlagManager {

        public static FeatureFlagManager instance => IoC.inject.Get<FeatureFlagManager>(null, false);

        private IKeyValueStore store;

        public FeatureFlagManager(IKeyValueStore featureFlagStore) { this.store = featureFlagStore; }

        public async Task<bool> IsEnabled(string featureId) {
            FeatureFlag flag = await GetFeatureFlag(featureId);
            if (flag == null) { return false; } // No feature returned so feature not enabled
            return flag.IsEnabled();
        }

        public async Task<FeatureFlag> GetFeatureFlag(string featureId) {
            FeatureFlag flag = await store.Get<FeatureFlag>(featureId, null);
            if (flag != null && flag.localState.randomPercentage == 0) {
                // if the server decided its a staged rollout no rnd % generated yet so do it:
                flag.localState.randomPercentage = new Random().Next(1, 100);
                await store.Set(featureId, flag.localState); // save in local store
            }
            return flag;
        }

    }

    public abstract class DefaultFeatureFlagStore : IKeyValueStore {

        public IKeyValueStore localStore { get; set; }
        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }

        public DefaultFeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) {
            this.localStore = localStore;
            this.fallbackStore = remoteStore;
        }

        protected abstract string GenerateFeatureKey(string featureId);

        public async Task<T> Get<T>(string featureId, T defaultValue) {
            if (typeof(T) != typeof(FeatureFlag)) {
                throw new NotSupportedException("Only Get<FeatureFlag> supported, not " + typeof(T));
            }
            var flag = await fallbackStore.Get<FeatureFlag>(GenerateFeatureKey(featureId), null);
            if (flag != null) { flag.localState = await localStore.Get(featureId, flag.localState); }
            return (T)(object)flag;
        }

        public Task<object> Set(string featureId, object value) {
            if (!(value is FeatureFlag.LocalState)) {
                throw new NotSupportedException("Can only persist FeatureFlag.LocalState, not " + value.GetType());
            }
            return localStore.Set(featureId, value);
        }

        public void Dispose() {
            localStore.Dispose();
            fallbackStore.Dispose();
        }

        public Task<bool> ContainsKey(string key) { throw new NotSupportedException(); }
        public Task<IEnumerable<string>> GetAllKeys() { throw new NotSupportedException(); }
        public Task<bool> Remove(string key) { throw new NotSupportedException(); }
        public Task RemoveAll() { throw new NotSupportedException(); }

    }

}