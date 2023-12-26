using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Zio;

namespace com.csutil.logging.analytics {
    
    public class LocalAnalyticsV2 : KeyValueStoreTypeAdapter<AppFlowEvent>, IDisposable {

        public readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores
            = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();
        
        private readonly DirectoryEntry _dir;

        public LocalAnalyticsV2(DirectoryEntry dir)
            : base(new ObservableKeyValueStore(new FileBasedKeyValueStore(dir))) {
            this._dir = dir;
        }

        public override async Task<AppFlowEvent> Set(string key, AppFlowEvent value) {
            var replacedEvent = await base.Set(key, value);
            await GetStoreForCategory(value.cat).Set(key, value);
            return replacedEvent;
        }

        public override async Task<bool> Remove(string key) {
            var res = await base.Remove(key);
            foreach (var s in categoryStores.Values) { res &= await s.Remove(key); }
            return res;
        }

        public override async Task RemoveAll() {
            await base.RemoveAll();
            foreach (var s in categoryStores.Values) { await s.RemoveAll(); }
        }

        public KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod) {
            if (categoryStores.TryGetValue(catMethod, out var store)) { return store; }
            var createdStore = new FileBasedKeyValueStore(_dir.GetChildDir(catMethod)).GetTypeAdapter<AppFlowEvent>();
            categoryStores.Add(catMethod, createdStore);
            return createdStore;
        }

        public void Dispose() {
            store.Dispose();
            foreach (var s in categoryStores.Values) { s.store.Dispose(); }
            categoryStores.Clear();
        }

    }
    
}