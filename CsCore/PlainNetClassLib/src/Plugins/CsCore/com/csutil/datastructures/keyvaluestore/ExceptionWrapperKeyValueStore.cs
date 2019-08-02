using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {
    public class ExceptionWrapperKeyValueStore : IKeyValueStore {
        private IKeyValueStore wrappedStore;
        public HashSet<Type> errorTypeBlackList;
        public Action<Exception> onError = (e) => { Log.e(e); };

        private static HashSet<Type> NewDefaultErrorSet() { return new HashSet<Type>() { typeof(InvalidCastException) }; }

        public ExceptionWrapperKeyValueStore(IKeyValueStore wrappedStore) : this(wrappedStore, NewDefaultErrorSet()) { }

        public ExceptionWrapperKeyValueStore(IKeyValueStore wrappedStore, HashSet<Type> errorTypeBlackList) {
            SetFallbackStore(wrappedStore);
            this.errorTypeBlackList = errorTypeBlackList;
        }

        public void SetFallbackStore(IKeyValueStore fallbackStore) { wrappedStore = fallbackStore; }

        private async Task<T> WrapWithTry<T>(Func<Task<T>> f, T defaultValue) {
            try { return await f(); } catch (Exception e) {
                if (IsOnBlackList(e)) { throw e; } else { onError.InvokeIfNotNull(e); }
            }
            return defaultValue;
        }

        public async Task<bool> ContainsKey(string key) {
            return await WrapWithTry(() => { return wrappedStore.ContainsKey(key); }, false);
        }

        private bool IsOnBlackList(Exception e) { return errorTypeBlackList.Contains(e.GetType()); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            return await WrapWithTry<T>(() => { return wrappedStore.Get<T>(key, defaultValue); }, defaultValue);
        }

        public async Task<bool> Remove(string key) {
            return await WrapWithTry(() => { return wrappedStore.Remove(key); }, false);
        }

        public Task RemoveAll() {
            try { return wrappedStore.RemoveAll(); } catch (Exception e) { return Task.FromException(e); }
        }

        public async Task<object> Set(string key, object obj) {
            return await WrapWithTry(() => { return wrappedStore.Set(key, obj); }, null);
        }

        public async Task<IEnumerable<string>> GetAllKeys() {
            return await WrapWithTry<IEnumerable<string>>(() => { return wrappedStore.GetAllKeys(); }, null);
        }

    }
}