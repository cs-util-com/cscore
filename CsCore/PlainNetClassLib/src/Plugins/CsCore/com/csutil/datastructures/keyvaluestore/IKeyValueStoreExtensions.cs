using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            AssertV2.IsTrue(res == JsonWriter.HasEqualJson(storeValue, defaultValue), $"Equals says {res} but EqualJson says {!res}!");
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
                        AssertV2.IsTrue(JsonWriter.HasEqualJson(storeOldValue, oldValue), "oldValue != store.oldValue, store value newer?"
                            + "\n storeOldValue=" + JsonWriter.AsPrettyString(storeOldValue) + "\n oldValue=" + JsonWriter.AsPrettyString(oldValue));
                    }
                }
            }
            return oldValue;
        }

        public static Stopwatch StartFallbackStoreGetTimer(this IKeyValueStore self) {
            if (self.fallbackStore != null) { return Stopwatch.StartNew(); } else { return null; }
        }

        public static async Task WaitLatestFallbackGetTime<T>(this IKeyValueStore self, Stopwatch s, Task<T> fallbackGet, float multFactor = 2f) {
            if (self.fallbackStore != null && s != null) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                fallbackGet.ContinueWithSameContext(_ => { // Use average of the last 2 requests:
                    self.latestFallbackGetTimingInMs = (self.latestFallbackGetTimingInMs + s.ElapsedMilliseconds) / 2;
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                await Task.WhenAny(fallbackGet, Task.Delay((int)(self.latestFallbackGetTimingInMs * multFactor)));
            }
        }

        public static async Task<IEnumerable<T>> GetAll<T>(this IKeyValueStore self) {
            return await (await self.GetAllKeys()).MapAsync(key => self.Get<T>(key, default(T)));
        }

        public static KeyValueStoreTypeAdapter<T> GetTypeAdapter<T>(this IKeyValueStore self) {
            return new KeyValueStoreTypeAdapter<T>(self);
        }

    }

}