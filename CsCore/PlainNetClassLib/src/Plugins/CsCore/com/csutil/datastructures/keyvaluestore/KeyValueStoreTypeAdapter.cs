using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class KeyValueStoreTypeAdapter<T> : IKeyValueStoreTypeAdapter<T>, IDisposableV2 {

        public IKeyValueStore store { get; set; }

        public KeyValueStoreTypeAdapter(IKeyValueStore store) { this.store = store; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;
        
        public virtual void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            store.DisposeV2();
            IsDisposed = DisposeState.Disposed;
        }
        
        public virtual Task<T> Get(string key, T defVal) { return store.Get(key, defVal); }

        public virtual async Task<T> Set(string key, T val) {
            object o = await store.Set(key, val);
            if (o == null) { return default(T); }
            return (T)o;
        }

        public virtual Task<bool> Remove(string key) { return store.Remove(key); }

        public virtual Task RemoveAll() { return store.RemoveAll(); }

        public virtual Task<bool> ContainsKey(string key) { return store.ContainsKey(key); }

        public virtual async Task<IEnumerable<string>> GetAllKeys() { return (await store.GetAllKeys()).Cached(); }

        public virtual Task<IEnumerable<T>> GetAll() { return store.GetAll<T>(); }

    }

}