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
                if (!IsDefaultValue(storeValue, defaultValue)) { onGetSuccess(storeValue); }
                return storeValue;
            }
            return defaultValue;
        }

        private static bool IsDefaultValue<T>(T storeValue, T defaultValue) {
            var res = Equals(storeValue, defaultValue);
            AssertV2.IsTrue(res == CloneHelper.HasEqualJson(storeValue, defaultValue), $"Equals says {res} but EqualJson says {!res}!");
            return res;
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
                        AssertV2.IsTrue(CloneHelper.HasEqualJson(storeOldValue, oldValue), "oldValue != store.oldValue, store value newer?"
                            + "\n storeOldValue=" + JsonWriter.AsPrettyString(storeOldValue) + "\n oldValue=" + JsonWriter.AsPrettyString(oldValue));
                    }
                }
            }
            return oldValue;
        }

    }

}