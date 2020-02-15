using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace com.csutil.keyvaluestore {

    public class InMemoryKeyValueStore : IKeyValueStore {

        private Dictionary<string, object> store = new Dictionary<string, object>();
        public IKeyValueStore fallbackStore { get; set; }

        public async Task<T> Get<T>(string key, T defaultValue) {
            object value;
            if (store.TryGetValue(key, out value)) { return (T)value; }
            return await fallbackStore.Get(key, defaultValue, (res) => InternalSet(key, res));
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