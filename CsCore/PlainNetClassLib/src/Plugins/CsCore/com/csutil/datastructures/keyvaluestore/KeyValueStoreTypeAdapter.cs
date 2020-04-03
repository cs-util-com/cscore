using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class KeyValueStoreTypeAdapter<T> {

        public IKeyValueStore store { get; set; }

        public KeyValueStoreTypeAdapter(IKeyValueStore store) {
            this.store = store;
        }

        public Task<T> Get(string key, T defaultValue) {
            return store.Get<T>(key, defaultValue);
        }

        public async Task<T> Set(string key, T value) {
            return (T)await store.Set(key, value);
        }

        public Task<bool> Remove(string key) {
            return store.Remove(key);
        }

        public Task RemoveAll() {
            return store.RemoveAll();
        }

        public Task<bool> ContainsKey(string key) {
            return store.ContainsKey(key);
        }

        public Task<IEnumerable<string>> GetAllKeys() {
            return store.GetAllKeys();
        }

        public Task<IEnumerable<T>> GetAll() {
            return store.GetAll<T>();
        }

    }

}
