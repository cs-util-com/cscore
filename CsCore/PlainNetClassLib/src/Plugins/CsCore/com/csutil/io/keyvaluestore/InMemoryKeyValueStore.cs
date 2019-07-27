using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.io.keyvaluestore {
    public class InMemoryKeyValueStore : IKeyValueStore {
        private Dictionary<string, object> store = new Dictionary<string, object>();
        private IKeyValueStore fallbackStore;

        public Task<T> Get<T>(string key, T defaultValue) {
            object value;
            if (store.TryGetValue(key, out value) && value is T) { return Task.FromResult((T)value); }
            if (fallbackStore != null) { return fallbackStore.Get<T>(key, defaultValue); }
            return Task.FromResult(defaultValue);
        }

        public async Task Set(string key, object obj) {
            store.AddOrReplace(key, obj);
            if (fallbackStore != null) { await fallbackStore.Set(key, obj); }
        }

        public void SetFallbackStore(IKeyValueStore fallbackStore) { this.fallbackStore = fallbackStore; }
    }
}