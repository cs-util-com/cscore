using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;

namespace com.csutil.model {

    public abstract class BaseFeatureFlagStore<T, V> : KeyValueStoreTypeAdapter<T>, IDisposable where T : IFeatureFlag where V : IFeatureFlagLocalState {

        private IKeyValueStore localStore;

        public BaseFeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) : base(remoteStore) {
            this.localStore = localStore;
        }

        /// <summary>
        ///  Here e.g the EnvironmentV2.instance.systemInfo.osPlatform could be added to
        ///  allow different rollout percentages for different target platforms
        /// </summary>
        protected abstract string GenerateFeatureKey(string featureId);

        public override async Task<T> Get(string featureId, T defVal) {
            var flag = await base.Get(GenerateFeatureKey(featureId), defVal);
            if (flag != null) { flag.localState = await localStore.Get(featureId, (V)flag.localState); }
            return flag;
        }

        public override async Task<T> Set(string featureId, T value) {
            await localStore.Set(featureId, value.localState);
            return default(T);
        }

        public void Dispose() {
            store.Dispose();
            localStore.Dispose();
        }

    }

}