using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    [Obsolete("Use LocalAnalyticsV2 instead", true)]
    public class LocalAnalytics : KeyValueStoreTypeAdapter<AppFlowEvent>, ILocalAnalytics {

        private const string DEFAULT_DIR = "AppFlowAnalytics";

        public IReadOnlyDictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores => _categoryStores;
        
        private readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> _categoryStores
            = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();

        public Func<string, KeyValueStoreTypeAdapter<AppFlowEvent>> createStoreFor = (dirName) => {
            return FileBasedKeyValueStore.New(DEFAULT_DIR + "_" + dirName).GetTypeAdapter<AppFlowEvent>();
        };

        public LocalAnalytics(string dirName = DEFAULT_DIR)
            : this(new ObservableKeyValueStore(FileBasedKeyValueStore.New(dirName))) {
        }

        public LocalAnalytics(IKeyValueStore mainStore) : base(mainStore) { }

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
            if (_categoryStores.TryGetValue(catMethod, out var store)) { return store; }
            var createdStore = createStoreFor(catMethod);
            _categoryStores.Add(catMethod, createdStore);
            return createdStore;
        }

        public void Dispose() {
            store.Dispose();
            foreach (var s in _categoryStores.Values) { s.store.Dispose(); }
            _categoryStores.Clear();
        }

    }

}