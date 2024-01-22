using com.csutil.keyvaluestore;
using System;
using System.Threading.Tasks;

namespace com.csutil.model {

    public abstract class BaseFeatureFlagStore<T, V> : KeyValueStoreTypeAdapter<T>, IDisposableV2
                                            where T : IFeatureFlag where V : IFeatureFlagLocalState {

        private readonly IKeyValueStore localStore;
        
        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public BaseFeatureFlagStore(IKeyValueStore localStore, IKeyValueStore remoteStore) : base(remoteStore) {
            this.localStore = localStore;
        }
        
        public override void Dispose() {
            base.Dispose();
            IsDisposed = DisposeState.DisposingStarted;
            store.DisposeV2();
            localStore.DisposeV2();
            IsDisposed = DisposeState.Disposed;
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
            // Only the localState is stored to the local store, the rest skipped
            await localStore.Set(featureId, value.localState);
            return default(T); // No update of value so nothing old to return 
        }

    }

}