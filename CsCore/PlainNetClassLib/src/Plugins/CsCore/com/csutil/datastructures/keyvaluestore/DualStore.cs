using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace com.csutil.keyvaluestore {

    public class DualStore : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        public IKeyValueStore secondStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }

        public DualStore(IKeyValueStore firstStore, IKeyValueStore secondStore) {
            fallbackStore = firstStore;
            this.secondStore = secondStore;
        }

        public async Task<bool> ContainsKey(string key) {
            var task1 = fallbackStore.ContainsKey(key);
            var task2 = secondStore.ContainsKey(key);
            var anyTask = Task.WhenAny(task1, task2).Unwrap();
            if (await anyTask) { return true; }
            if (await task2) { return true; }
            return await task1;
        }

        public Task<object> Set(string key, object value) {
            return fallbackStore.Set(key, value);
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var task1 = fallbackStore.Get(key, defaultValue);
            var task2 = secondStore.Get(key, defaultValue);
            var anyTask = await Task.WhenAny(task1, task2).Unwrap();
            if (!Equals(anyTask, defaultValue)) { return anyTask; }
            var t2Result = await task2;
            if (!Equals(t2Result, defaultValue)) { return t2Result; }
            return await task1;
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            var a = fallbackStore.GetAllKeys();
            var b = await secondStore.GetAllKeys();
            return b.Union(await a);
        }

        public async Task<bool> Remove(string key) {
            var a = fallbackStore.Remove(key);
            var b = await secondStore.Remove(key);
            return (await a) || b;
        }

        public async Task RemoveAll() {
            var a = fallbackStore.RemoveAll();
            await secondStore.RemoveAll();
            await a;
        }

        public void Dispose() {
            fallbackStore.Dispose();
            secondStore.Dispose();
        }

    }

}