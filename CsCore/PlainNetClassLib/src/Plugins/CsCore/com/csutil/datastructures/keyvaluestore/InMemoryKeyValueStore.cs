using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class InMemoryKeyValueStore : IKeyValueStore {

        private Dictionary<string, object> store = new Dictionary<string, object>();
        private IKeyValueStore fallbackStore;

        public async Task<T> Get<T>(string key, T defaultValue) {
            object value;
            if (store.TryGetValue(key, out value) && value is T) { return (T)value; }
            if (fallbackStore != null) {
                var fallbackValue = await fallbackStore.Get<T>(key, defaultValue);
                if (!ReferenceEquals(fallbackValue, defaultValue)) { InternalSet(key, fallbackValue); }
                return fallbackValue;
            }
            return defaultValue;
        }

        public async Task<bool> ContainsKey(string key) {
            if (store.ContainsKey(key)) { return true; }
            if (fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return false;
        }

        public async Task<object> Set(string key, object obj) {
            var oldEntry = InternalSet(key, obj);
            if (fallbackStore != null) {
                var fallbackOldEntry = await fallbackStore.Set(key, obj);
                if (oldEntry == null && fallbackOldEntry != null) { oldEntry = fallbackOldEntry; }
            }
            return oldEntry;
        }

        private object InternalSet(string key, object obj) { return store.AddOrReplace(key, obj); }

        public async Task<bool> Remove(string key) {
            var res = store.Remove(key);
            if (fallbackStore != null) { res &= await fallbackStore.Remove(key); }
            return res;
        }

        public async Task RemoveAll() {
            store.Clear();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public void SetFallbackStore(IKeyValueStore fallbackStore) { this.fallbackStore = fallbackStore; }

    }
}