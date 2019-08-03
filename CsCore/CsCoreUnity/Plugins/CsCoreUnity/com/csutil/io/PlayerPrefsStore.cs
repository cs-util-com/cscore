using com.csutil.json;
using com.csutil.keyvaluestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.io {

    public class PlayerPrefsStore : IKeyValueStore {

        public IKeyValueStore fallbackStore { get; set; }
        private IJsonReader jsonReader = TypedJsonHelper.NewTypedJsonReader();
        private IJsonWriter jsonWriter = TypedJsonHelper.NewTypedJsonWriter();

        public async Task<T> Get<T>(string key, T defaultValue) {
            T value = (T)InternalGet(key, defaultValue);
            if (!ReferenceEquals(value, defaultValue)) { return value; }
            return await fallbackStore.Get(key, defaultValue, (res) => InternalSet(key, res));
        }

        public async Task<object> Set(string key, object value) {
            var oldValue = InternalSet(key, value);
            return await fallbackStore.Set(key, value, oldValue);
        }

        private object InternalSet(string key, object value) {
            object oldValue = InternalGet(key, null);
            PlayerPrefs.SetString(key, ToJsonString(value));
            return oldValue;
        }

        private string ToJsonString(object value) {
            return jsonWriter.Write(new ValueWrapper() { value = value });
        }

        private object InternalGet(string key, object defaultValue) {
            if (!PlayerPrefs.HasKey(key)) { return defaultValue; }
            return jsonReader.Read<ValueWrapper>(PlayerPrefs.GetString(key)).value;
        }

        public async Task<bool> Remove(string key) {
            PlayerPrefs.DeleteKey(key);
            if (fallbackStore != null) { return await fallbackStore.Remove(key); }
            return true;
        }

        public async Task RemoveAll() {
            PlayerPrefs.DeleteAll();
            if (fallbackStore != null) { await fallbackStore.RemoveAll(); }
        }

        public async Task<bool> ContainsKey(string key) {
            var res = PlayerPrefs.HasKey(key);
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
