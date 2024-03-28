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

        public static bool RemoveFromValues<T, V>(this IDictionary<T, HashSet<V>> self, T key, V value) {
            if (self.TryGetValue(key, out HashSet<V> values)) {
                if (values.Remove(value)) {
                    if (values.IsEmpty()) {
                        self.Remove(key);
                    } else {
                        self[key] = values;
                    }
                    return true;
                }
            }
            return false;
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

        public static bool AddRange<T>(this ISet<T> self, IEnumerable<T> e) {
            bool res = true;
            foreach (var l in e) { res &= self.Add(l); }
            return res;
        }
        
        public static bool RemoveRange<T>(this ISet<T> self, IEnumerable<T> e) {
            bool res = true;
            foreach (var l in e) { res &= self.Remove(l); }
            return res;
        }

        public static void AddRangeOverride<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> rangeToAdd) {
            foreach (var e in rangeToAdd) {
                self[e.Key] = e.Value;
            }
        }

        public static void AddRangeNewOnly<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> rangeToAdd) {
            foreach (var e in rangeToAdd) {
                if (!self.ContainsKey(e.Key)) {
                    self.Add(e.Key, e.Value);
                }
            }
        }

        public static void AddRange<K, V>(this IDictionary<K, V> self, IEnumerable<KeyValuePair<K, V>> rangeToAdd) {
            foreach (var e in rangeToAdd) {
                self.Add(e.Key, e.Value);
            }
        }

    }

}