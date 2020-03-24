
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class RetryKeyValueStore : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }
        public int maxNrOfRetries;
        public int maxDelayInMs = -1;
        public int initialExponent = 0;

        /// <summary> 
        /// Every time the retry store catches an error and retries the error is send here e.g. for 
        /// logging or re-throwing (to cancel retries on certain errors) 
        /// </summary>
        public Action<Exception> onError;

        public RetryKeyValueStore(IKeyValueStore wrappedStore, int maxNrOfRetries) {
            this.fallbackStore = wrappedStore;
            this.maxNrOfRetries = maxNrOfRetries;
        }

        public void Dispose() { fallbackStore?.Dispose(); }

        public Task<T> Retry<T>(Func<Task<T>> taskToTry) {
            return TaskV2.TryWithExponentialBackoff<T>(taskToTry, onError, maxNrOfRetries, maxDelayInMs, initialExponent);
        }

        public Task<bool> ContainsKey(string key) { return Retry(() => fallbackStore.ContainsKey(key)); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = Stopwatch.StartNew();
            var res = await Retry(() => fallbackStore.Get<T>(key, defaultValue));
            latestFallbackGetTimingInMs = s.ElapsedMilliseconds;
            return res;
        }

        public Task<IEnumerable<string>> GetAllKeys() { return Retry(() => fallbackStore.GetAllKeys()); }

        public Task<bool> Remove(string key) { return Retry(() => fallbackStore.Remove(key)); }

        public Task RemoveAll() { return Retry<bool>(async () => { await fallbackStore.RemoveAll(); return true; }); }

        public Task<object> Set(string key, object value) { return Retry(() => fallbackStore.Set(key, value)); }

    }

}