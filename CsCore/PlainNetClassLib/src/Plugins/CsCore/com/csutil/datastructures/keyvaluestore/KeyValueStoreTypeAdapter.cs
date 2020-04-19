using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class KeyValueStoreTypeAdapter<T> {

        public IKeyValueStore store { get; set; }

        public KeyValueStoreTypeAdapter(IKeyValueStore store) { this.store = store; }

        public virtual Task<T> Get(string key, T defVal) { return store.Get(key, defVal); }

        public virtual async Task<T> Set(string key, T val) { return (T)await store.Set(key, val); }

        public virtual Task<bool> Remove(string key) { return store.Remove(key); }

        public virtual Task RemoveAll() { return store.RemoveAll(); }

        public virtual Task<bool> ContainsKey(string key) { return store.ContainsKey(key); }

        public virtual Task<IEnumerable<string>> GetAllKeys() { return store.GetAllKeys(); }

        public virtual Task<IEnumerable<T>> GetAll() { return store.GetAll<T>(); }

    }

}