using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;

namespace com.csutil.model {

    public class DefaultFeatureFlagStore : FeatureFlagStore {

        public DefaultFeatureFlagStore(IKeyValueStore l, IKeyValueStore r) : base(l, r) { }

        // By default just return the featureId:
        protected override string GenerateFeatureKey(string featureId) { return featureId; }

    }

    public abstract class FeatureFlagStore : KeyValueStoreTypeAdapter<FeatureFlag>, IDisposable {

        private IKeyValueStore localStore;

        public FeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) : base(remoteStore) {
            this.localStore = localStore;
        }

        /// <summary>
        ///  Here e.g the EnvironmentV2.instance.systemInfo.osPlatform could be added to
        ///  allow different rollout percentages for different target platforms
        /// </summary>
        protected abstract string GenerateFeatureKey(string featureId);

        public override async Task<FeatureFlag> Get(string featureId, FeatureFlag defVal) {
            var flag = await base.Get(GenerateFeatureKey(featureId), defVal);
            if (flag != null) { flag.localState = await localStore.Get(featureId, flag.localState); }
            return flag;
        }

        public override async Task<FeatureFlag> Set(string featureId, FeatureFlag value) {
            await localStore.Set(featureId, value.localState);
            return null;
        }

        public void Dispose() {
            store.Dispose();
            localStore.Dispose();
        }

    }

}