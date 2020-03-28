using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace com.csutil.keyvaluestore {

    public class InMemoryKeyValueStore : IKeyValueStore {

        private Dictionary<string, object> store;

        public long latestFallbackGetTimingInMs { get; set; }

        public IKeyValueStore fallbackStore { get; set; }

        public InMemoryKeyValueStore() : this(new Dictionary<string, object>()) { }

        public InMemoryKeyValueStore(Dictionary<string, object> store) { this.store = store; }

        public void Dispose() {
            // TODO iterate over entries, and dispose them if possible?
            store.Clear();
            store = null;
            fallbackStore?.Dispose();
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = this.StartFallbackStoreGetTimer();
            Task<T> fallbackGet = fallbackStore.Get(key, defaultValue, (newVal) => InternalSet(key, newVal));
            await this.WaitLatestFallbackGetTime(s, fallbackGet);
            if (store.TryGetValue(key, out object value)) { return (T)value; }
            return await fallbackGet;
        }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) { return store.AddOrReplace(key, value); }

        public async Task<bool> Remove(string key) {
            var res = store.Remove(key);
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            store.Clear();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            if (store.ContainsKey(key)) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<IEnumerable<string>> GetAllKeys() { return await fallbackStore.ConcatAllKeys(store.Keys); }

    }

}