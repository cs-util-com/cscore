
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.async;

namespace com.csutil.keyvaluestore {

    public class RetryKeyValueStore : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        public int maxNrOfRetries;
        public int maxDelayInMs = -1;
        public int initialExponent = 0;
        public Action<Exception> onError;

        public RetryKeyValueStore(IKeyValueStore wrappedStore, int maxNrOfRetries) {
            this.fallbackStore = wrappedStore;
            this.maxNrOfRetries = maxNrOfRetries;
        }

        public Task<T> Retry<T>(Func<Task<T>> taskToTry) {
            return TaskHelper.TryWithExponentialBackoff<T>(taskToTry, onError, maxNrOfRetries, maxDelayInMs, initialExponent);
        }

        public Task<bool> ContainsKey(string key) { return Retry(() => fallbackStore.ContainsKey(key)); }

        public Task<T> Get<T>(string key, T defaultValue) { return Retry(() => fallbackStore.Get<T>(key, defaultValue)); }

        public Task<IEnumerable<string>> GetAllKeys() { return Retry(() => fallbackStore.GetAllKeys()); }

        public Task<bool> Remove(string key) { return Retry(() => fallbackStore.Remove(key)); }

        public Task RemoveAll() { return Retry<bool>(async () => { await fallbackStore.RemoveAll(); return true; }); }

        public Task<object> Set(string key, object value) { return Retry(() => fallbackStore.Set(key, value)); }

    }

}