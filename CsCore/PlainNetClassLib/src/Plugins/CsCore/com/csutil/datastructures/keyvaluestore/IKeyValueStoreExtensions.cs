using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace com.csutil.keyvaluestore {

    public static class IKeyValueStoreExtensions {

        public static T WithFallbackStore<T>(this T self, IKeyValueStore fallbackStore) where T : IKeyValueStore {
            self.fallbackStore = fallbackStore;
            return self;
        }

        public static async Task<T> Get<T>(this IKeyValueStore self, string key, T defaultValue, Action<object> onGetSuccess) {
            if (self != null) {
                var storeValue = await self.Get<T>(key, defaultValue);
                if (!Equals(storeValue, defaultValue)) { onGetSuccess(storeValue); }
                return storeValue;
            }
            return defaultValue;
        }

        public static async Task<IEnumerable<string>> ConcatAllKeys(this IKeyValueStore self, IEnumerable<string> result) {
            if (self != null) {
                var storeKeys = await self.GetAllKeys();
                if (storeKeys != null) {
                    var filteredStoreKeys = (storeKeys).Filter(e => !result.Contains(e));
                    result = result.Concat(filteredStoreKeys);
                }
            }
            return result;
        }

        public static async Task<object> Set(this IKeyValueStore self, string key, object newValue, object oldValue) {
            if (self != null) {
                var storeOldValue = await self.Set(key, newValue);
                if (storeOldValue != null) {
                    if (oldValue == null) { oldValue = storeOldValue; }
                    if (storeOldValue != oldValue) {
                        AssertV2.IsTrue(storeOldValue.Equals(oldValue), "oldValue != store.oldValue, store value newer?"
                            + "\n storeOldValue=" + storeOldValue + "\n oldValue=" + oldValue);
                    }
                }
            }
            return oldValue;
        }

    }

}