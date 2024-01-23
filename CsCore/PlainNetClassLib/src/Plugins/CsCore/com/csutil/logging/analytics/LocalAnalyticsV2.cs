using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Zio;

namespace com.csutil.logging.analytics {

    [Obsolete("Use LocalAnalyticsV3 instead")]
    public class LocalAnalyticsV2 : KeyValueStoreTypeAdapter<AppFlowEvent>, ILocalAnalytics {

        public IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores => _categoryStores;

        private readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> _categoryStores
            = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();

        private readonly DirectoryEntry _dir;
        private readonly object _threadLock = new object();

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public LocalAnalyticsV2(DirectoryEntry dirForEvents)
            : base(new ObservableKeyValueStore(new FileBasedKeyValueStore(dirForEvents))) {
            this._dir = dirForEvents;
        }

        public override void Dispose() {
            base.Dispose();
            IsDisposed = DisposeState.DisposingStarted;
            store.DisposeV2();
            foreach (var s in categoryStores.Values) {
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
            lock (_threadLock) {
                if (_categoryStores.TryGetValue(catMethod, out var store)) { return store; }
                var createdStore = new FileBasedKeyValueStore(_dir.GetChildDir(catMethod)).GetTypeAdapter<AppFlowEvent>();
                _categoryStores.Add(catMethod, createdStore);
                return createdStore;
            }
        }

    }

}