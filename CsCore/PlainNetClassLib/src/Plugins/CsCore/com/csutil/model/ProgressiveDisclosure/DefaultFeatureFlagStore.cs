using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;

namespace com.csutil.model {

    public abstract class DefaultFeatureFlagStore : KeyValueStoreTypeAdapter<FeatureFlag>, IDisposable {

        private IKeyValueStore localStore;

        public DefaultFeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) : base(remoteStore) {
            this.localStore = localStore;
        }

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