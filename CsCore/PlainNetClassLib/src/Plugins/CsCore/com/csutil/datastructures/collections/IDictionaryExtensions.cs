using System.Collections.Generic;
using System.Linq;

namespace com.csutil {

    public static class IDictionaryExtensions {

        /// <summary> Returns false if an existing value was replaced </summary>
        public static V AddOrReplace<T, V>(this IDictionary<T, V> self, T key, V value) {
            if (self.ContainsKey(key)) {
                var oldV = self[key];
                self[key] = value;
                return oldV;
            }
            self.Add(key, value);
            return default(V);
        }

        /// <summary> Allows to add values to maps that support multiple values per key </summary>
        public static void AddToValues<T, V>(this IDictionary<T, HashSet<V>> self, T key, V value) {
            if (!self.ContainsKey(key)) { self.Add(key, new HashSet<V>()); }
            self[key].Add(value);
        }

        public static V GetValue<T, V>(this IDictionary<T, V> self, T key, V fallback) {
            return self.TryGetValue(key, out V value) ? value : fallback;
        }

        public static IDictionary<T, V> ExceptKeys<T, V>(this IDictionary<T, V> self, IDictionary<T, V> otherDict) {
            return self.Keys.Except(otherDict.Keys).ToDictionary(k => k, k => self[k]);
        }

        public static IDictionary<T, V> IntersectKeys<T, V>(this IDictionary<T, V> self, IDictionary<T, V> otherDict) {
            return self.Keys.Intersect(otherDict.Keys).ToDictionary(k => k, k => self[k]);
        }

    }

}