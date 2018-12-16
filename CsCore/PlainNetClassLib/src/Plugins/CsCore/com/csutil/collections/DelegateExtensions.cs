namespace System {
    public static class DelegateExtensions {

        public static bool InvokeIfNotNull(this Action a) { if (a == null) { return false; } else { a.Invoke(); return true; } }
        public static bool InvokeIfNotNull<T>(this Action<T> a, T t) { if (a == null) { return false; } else { a.Invoke(t); return true; } }
        public static bool InvokeIfNotNull<T, V>(this Action<T, V> a, T t, V v) { if (a == null) { return false; } else { a.Invoke(t, v); return true; } }
        public static bool InvokeIfNotNull<T, V, U>(this Action<T, V, U> a, T t, V v, U u) { if (a == null) { return false; } else { a.Invoke(t, v, u); return true; } }

    }
}