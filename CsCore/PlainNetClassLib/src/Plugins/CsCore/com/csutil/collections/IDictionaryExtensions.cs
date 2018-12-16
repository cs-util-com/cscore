namespace System.Collections.Generic {
    public static class IDictionaryExtensions {
        public static bool AddOrReplace<T, V>(this IDictionary<T, V> self, T key, V value) {
            if (self.ContainsKey(key)) { self[key] = value; return false; }
            self.Add(key, value);
            return true;
        }

    }
}