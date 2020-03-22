using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class ExceptionWrapperKeyValueStore : IKeyValueStore {

        public HashSet<Type> errorTypeBlackList;
        public Action<Exception> onError = (e) => { Log.e(e); };
        private IKeyValueStore wrappedStore;
        public long latestFallbackGetTimingInMs { get; set; }
        public IKeyValueStore fallbackStore {
            get { return wrappedStore; }
            set {
                if (value == null) { throw new ArgumentNullException("The target store cant be null"); }
                wrappedStore = value;
            }
        }

        public void Dispose() { wrappedStore.Dispose(); }

        private static HashSet<Type> NewDefaultErrorSet() {
            return new HashSet<Type>() { typeof(InvalidCastException), typeof(NotImplementedException) };
        }

        public ExceptionWrapperKeyValueStore(IKeyValueStore wrappedStore) : this(wrappedStore, NewDefaultErrorSet()) { }

        public ExceptionWrapperKeyValueStore(IKeyValueStore wrappedStore, HashSet<Type> errorTypeBlackList) {
            fallbackStore = wrappedStore;
            this.errorTypeBlackList = errorTypeBlackList;
        }

        private async Task<T> WrapWithTry<T>(Func<Task<T>> f, T defaultValue) {
            try { return await f(); } catch (Exception e) {
                if (IsOnBlackList(e)) { throw; } else { onError.InvokeIfNotNull(e); }
            }
            return defaultValue;
        }

        private bool IsOnBlackList(Exception e) { return errorTypeBlackList.Contains(e.GetType()); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = Stopwatch.StartNew();
            T result = await WrapWithTry<T>(() => { return wrappedStore.Get<T>(key, defaultValue); }, defaultValue);
            latestFallbackGetTimingInMs = s.ElapsedMilliseconds;
            return result;
        }

        public async Task<object> Set(string key, object obj) {
            return await WrapWithTry(() => { return wrappedStore.Set(key, obj); }, null);
        }

        public async Task<bool> Remove(string key) {
            return await WrapWithTry(() => { return wrappedStore.Remove(key); }, false);
        }

        public Task RemoveAll() {
            try { return wrappedStore.RemoveAll(); } catch (Exception e) { return Task.FromException(e); }
        }

        public async Task<bool> ContainsKey(string key) {
            return await WrapWithTry(() => { return wrappedStore.ContainsKey(key); }, false);
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            return await WrapWithTry<IEnumerable<string>>(() => { return wrappedStore.GetAllKeys(); }, null);
        }

    }

}