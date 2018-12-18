namespace System.Collections.Generic {
    public static class IDictionaryExtensions {

        public static bool AddOrReplace<T, V>(this IDictionary<T, V> self, T key, V value) {
            if (self.ContainsKey(key)) { self[key] = value; return false; }
            self.Add(key, value);
            return true;
        }

        public static V GetValue<T, V>(this IDictionary<T, V> self, T key, V fallback) {
            return self.TryGetValue(key, out V value) ? value : fallback;
        }

    }
}