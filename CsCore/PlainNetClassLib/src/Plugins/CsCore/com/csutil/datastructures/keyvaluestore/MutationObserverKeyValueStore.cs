using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class MutationObserverKeyValueStore : IKeyValueStore {

        public Func<string, object, object, Task> onSet;
        public Func<string, Task> onRemove;
        public Func<Task> onRemoveAll;

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs {
            get { return fallbackStore.latestFallbackGetTimingInMs; }
            set { fallbackStore.latestFallbackGetTimingInMs = value; }
        }

        public Task<bool> ContainsKey(string key) { return fallbackStore.ContainsKey(key); }

        public void Dispose() { fallbackStore.Dispose(); }

        public Task<T> Get<T>(string k, T def) { return fallbackStore.Get(k, def); }

        public Task<IEnumerable<string>> GetAllKeys() { return fallbackStore.GetAllKeys(); }

        public async Task<bool> Remove(string key) {
            if (!await fallbackStore.Remove(key)) { return false; }
            await onRemove?.Invoke(key);
            return true;
        }

        public async Task RemoveAll() {
            await fallbackStore.RemoveAll();
            await onRemoveAll?.Invoke();
        }

        public async Task<object> Set(string key, object value) {
            var replacedValue = await fallbackStore.Set(key, value);
            await onSet(key, value, replacedValue);
            return replacedValue;
        }

    }

}