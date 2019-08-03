namespace com.csutil.keyvaluestore {
    public static class IKeyValueStoreExtensions {

        public static T WithFallbackStore<T>(this T self, IKeyValueStore fallbackStore) where T : IKeyValueStore {
            self.fallbackStore = fallbackStore;
            return self;
        }

    }
}