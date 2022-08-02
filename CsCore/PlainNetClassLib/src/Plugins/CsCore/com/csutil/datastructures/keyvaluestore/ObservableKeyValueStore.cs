using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class ObservableKeyValueStore : IKeyValueStore, INotifyCollectionChanged {

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ObservableKeyValueStore(IKeyValueStore wrappedStore) { fallbackStore = wrappedStore; }

        public DisposeState IsDisposed { get; private set; } = DisposeState.Active;

        public void Dispose() {
            IsDisposed = DisposeState.DisposingStarted;
            fallbackStore?.Dispose();
            IsDisposed = DisposeState.Disposed;
        }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = Stopwatch.StartNew();
            T res = await fallbackStore.Get(key, defaultValue);
            latestFallbackGetTimingInMs = s.ElapsedMilliseconds;
            return res;
        }

        public async Task<object> Set(string key, object value) {
            var oldVal = await fallbackStore.Set(key, value);
            var newItem = new KeyValuePair<string, object>(key, value);
            if (oldVal == null) {
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem);
                CollectionChanged?.Invoke(this, e);
            } else {
                var oldItem = new KeyValuePair<string, object>(key, oldVal);
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem);
                CollectionChanged?.Invoke(this, e);
            }
            return oldVal;
        }

        public async Task<bool> Remove(string key) {
            var oldVal = fallbackStore.Get<object>(key, default);
            if (await fallbackStore.Remove(key)) {
                var oldItem = new KeyValuePair<string, object>(key, oldVal);
                var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, oldItem);
                CollectionChanged?.Invoke(this, e);
                return true;
            }
            return false;
        }

        public async Task RemoveAll() {
            await fallbackStore.RemoveAll();
            var e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove);
            CollectionChanged?.Invoke(this, e);
        }

        public Task<bool> ContainsKey(string key) { return fallbackStore.ContainsKey(key); }
        public Task<IEnumerable<string>> GetAllKeys() { return fallbackStore.GetAllKeys(); }

    }

}