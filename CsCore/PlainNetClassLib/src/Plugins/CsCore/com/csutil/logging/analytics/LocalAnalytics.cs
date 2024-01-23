using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    [Obsolete("Use LocalAnalyticsV3 instead", true)]
    public class LocalAnalytics : KeyValueStoreTypeAdapter<AppFlowEvent>, ILocalAnalytics {

        private const string DEFAULT_DIR = "AppFlowAnalytics";

        public IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores => _categoryStores;
        
        private readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> _categoryStores
            = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        public Func<string, KeyValueStoreTypeAdapter<AppFlowEvent>> createStoreFor = (dirName) => {
            return FileBasedKeyValueStore.New(DEFAULT_DIR + "_" + dirName).GetTypeAdapter<AppFlowEvent>();
        };

        public LocalAnalytics(string dirName = DEFAULT_DIR)
            : this(new ObservableKeyValueStore(FileBasedKeyValueStore.New(dirName))) {
        }

        public LocalAnalytics(IKeyValueStore mainStore) : base(mainStore) { }

        public override void Dispose() {
            base.Dispose();
            IsDisposed = DisposeState.DisposingStarted;
            store.DisposeV2();
            foreach (var s in _categoryStores.Values) {
                s.store.DisposeV2();
            }
            _categoryStores.Clear();
            IsDisposed = DisposeState.Disposed;
        }
        
        public override async Task<AppFlowEvent> Set(string key, AppFlowEvent value) {
            this.ThrowErrorIfDisposed();
            var replacedEvent = await base.Set(key, value);
            await GetStoreForCategory(value.cat).Set(key, value);
            return replacedEvent;
        }

        public override async Task<bool> Remove(string key) {
            this.ThrowErrorIfDisposed();
            var res = await base.Remove(key);
            foreach (var s in _categoryStores.Values) { res &= await s.Remove(key); }
            return res;
        }

        public override async Task RemoveAll() {
            this.ThrowErrorIfDisposed();
            await base.RemoveAll();
            foreach (var s in _categoryStores.Values) { await s.RemoveAll(); }
        }

        public KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod) {
            this.ThrowErrorIfDisposed();
            if (_categoryStores.TryGetValue(catMethod, out var store)) { return store; }
            var createdStore = createStoreFor(catMethod);
            _categoryStores.Add(catMethod, createdStore);
            return createdStore;
        }

    }

}