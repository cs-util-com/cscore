using com.csutil.json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public class PlayerPrefsStore : IKeyValueStore {

        /// <summary> 
        /// THe default store uses a Dictionary for memory caching and an exception layer to catch
        /// and errors that might happen e.g. when the PlayerPrefs are accessed from a background task
        /// </summary>
        public static IPreferences NewPreferencesUsingPlayerPrefs() {
            var wrappedPlayerPrefs = new ExceptionWrapperKeyValueStore(new PlayerPrefsStore());
            return new Preferences(new InMemoryKeyValueStore().WithFallbackStore(wrappedPlayerPrefs));
        }

        public IKeyValueStore fallbackStore { get; set; }
        public long latestFallbackGetTimingInMs { get; set; }
        private IJsonReader jsonReader = TypedJsonHelper.NewTypedJsonReader();
        private IJsonWriter jsonWriter = TypedJsonHelper.NewTypedJsonWriter();

        public void Dispose() { fallbackStore?.Dispose(); }

        public async Task<T> Get<T>(string key, T defaultValue) {
            var s = this.StartFallbackStoreGetTimer();
            var fallbackGet = fallbackStore.Get(key, defaultValue, (res) => InternalSet(key, res));
            await this.WaitLatestFallbackGetTime(s, fallbackGet);
            T value = InternalGet(key, defaultValue);
            if (!ReferenceEquals(value, defaultValue)) { return value; }
            return await fallbackGet;
        }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            object oldValue = InternalGet<object>(key, null);
            PlayerPrefsV2.SetString(key, ToJsonString(value));
            return oldValue;
        }

        private string ToJsonString(object value) {
            return jsonWriter.Write(new ValueWrapper() { value = value });
        }

        private T InternalGet<T>(string key, T defaultValue) {
            if (!PlayerPrefsV2.HasKey(key)) { return defaultValue; }
            var value = PlayerPrefsV2.GetString(key);
            var wrapper = jsonReader.Read<ValueWrapper>(value);
            if (wrapper == null) {
                Log.e($"Entry not a ValueWrapper but instead: '{value}'");
                return defaultValue;
            }
            return wrapper.GetValueAs<T>();
        }

        public async Task<bool> Remove(string key) {
            PlayerPrefsV2.DeleteKey(key);
            if (fallbackStore != null) { return await fallbackStore.Remove(key); }
            return true;
        }

        public async Task RemoveAll() {
            PlayerPrefsV2.DeleteAll();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            var res = PlayerPrefsV2.HasKey(key);
            if (!res && fallbackStore != null) { return await fallbackStore.ContainsKey(key); }
            return res;
        }

        public Task<IEnumerable<string>> GetAllKeys() {
            // If possible pass the request directly through to the fallback store:
            if (fallbackStore != null) { return fallbackStore.GetAllKeys(); }
            throw new NotSupportedException("PlayerPrefs does not support GetAllKeys()");
        }

    }

}
