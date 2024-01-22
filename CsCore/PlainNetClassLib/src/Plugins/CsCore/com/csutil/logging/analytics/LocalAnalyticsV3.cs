using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Zio;

namespace com.csutil.logging.analytics {
    
    public class LocalAnalyticsV3 : KeyValueStoreTypeAdapter<AppFlowEvent>, ILocalAnalytics {

        /// <summary> If the last 10 logs are missing if the app crashes this is probably ok, and not storing after every event increases performance </summary>
        private const int maxAllowedOpenChanges = 10;

        public IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores => _categoryStores;

        private readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> _categoryStores
            = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();
        
        private readonly DirectoryEntry _dir;
        private readonly object _threadLock = new object();

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        public LocalAnalyticsV3(DirectoryEntry dirForEvents)
            : base(new ObservableKeyValueStore(ZipFileBasedKeyValueStore.New(dirForEvents.CreateV2().GetChild("AllEvents.zip"), maxAllowedOpenChanges))) {
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
            var replacedEvent = await base.Set(key, value);
            await GetStoreForCategory(value.cat).Set(key, value);
            return replacedEvent;
        }

        public override async Task<bool> Remove(string key) {
            var res = await base.Remove(key);
            foreach (var s in _categoryStores.Values) { res &= await s.Remove(key); }
            return res;
        }

        public override async Task RemoveAll() {
            await base.RemoveAll();
            foreach (var s in _categoryStores.Values) { await s.RemoveAll(); }
        }

        public KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod) {
            lock (_threadLock) {
                if (_categoryStores.TryGetValue(catMethod, out var store)) { return store; }
                var createdStore = ZipFileBasedKeyValueStore.New(_dir.GetChild(catMethod + ".zip"), maxAllowedOpenChanges).GetTypeAdapter<AppFlowEvent>();
                _categoryStores.Add(catMethod, createdStore);
                return createdStore;
            }
        }

    }
    
}