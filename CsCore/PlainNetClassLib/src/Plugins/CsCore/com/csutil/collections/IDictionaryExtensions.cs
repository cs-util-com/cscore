namespace System.Collections.Generic {
    public static class IDictionaryExtensions {

        /// <summary> Returns false if an existing value was replaced </summary>
        public static bool AddOrReplace<T, V>(this IDictionary<T, V> self, T key, V value) {
            if (self.ContainsKey(key)) { self[key] = value; return false; }
            self.Add(key, value);
            return true;
        }

        public static V GetValue<T, V>(this IDictionary<T, V> self, T key, V fallback) {
            V value; return self.TryGetValue(key, out value) ? value : fallback;
        }

    }
}