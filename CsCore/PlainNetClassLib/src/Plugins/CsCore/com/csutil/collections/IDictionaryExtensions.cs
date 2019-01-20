using System.Collections.Generic;

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

        public static V GetValue<T, V>(this IDictionary<T, V> self, T key, V fallback) {
            V value; return self.TryGetValue(key, out value) ? value : fallback;
        }

    }

}