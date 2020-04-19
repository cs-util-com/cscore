using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;

namespace com.csutil.logging.analytics {

    public class LocalAnalytics : KeyValueStoreTypeAdapter<AppFlowEvent>, IDisposable {

        private const string DEFAULT_DIR = "AppFlowAnalytics";

        public readonly Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>> categoryStores
                = new Dictionary<string, KeyValueStoreTypeAdapter<AppFlowEvent>>();

        public Func<string, KeyValueStoreTypeAdapter<AppFlowEvent>> createStoreFor = (dirName) => {
            return FileBasedKeyValueStore.New(DEFAULT_DIR + "_" + dirName).GetTypeAdapter<AppFlowEvent>();
        };

        public LocalAnalytics(string dirName = DEFAULT_DIR)
            : this(FileBasedKeyValueStore.New(dirName)) {
        }

        public LocalAnalytics(IKeyValueStore mainStore) : base(mainStore) { }

        public override async Task<AppFlowEvent> Set(string key, AppFlowEvent value) {
            var replacedEvent = await base.Set(key, value);
            await GetStoreForCategory(value.cat).Set(key, value);
            return replacedEvent;
        }

        public override async Task<bool> Remove(string key) {
            var res = await base.Remove(key);
            foreach (IKeyValueStore s in categoryStores.Values) { res &= await s.Remove(key); }
            return res;
        }

        public override async Task RemoveAll() {
            await base.RemoveAll();
            foreach (IKeyValueStore s in categoryStores.Values) { await s.RemoveAll(); }
        }

        public KeyValueStoreTypeAdapter<AppFlowEvent> GetStoreForCategory(string catMethod) {
            if (categoryStores.TryGetValue(catMethod, out KeyValueStoreTypeAdapter<AppFlowEvent> store)) { return store; }
            var createdStore = createStoreFor(catMethod);
            categoryStores.Add(catMethod, createdStore);
            return createdStore;
        }

        public void Dispose() {
            store.Dispose();
            foreach (IKeyValueStore s in categoryStores.Values) { s.Dispose(); }
            categoryStores.Clear();
        }

    }

}