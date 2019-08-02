namespace com.csutil.keyvaluestore {
    public static class IKeyValueStoreExtensions {

        public static IKeyValueStore WithFallbackStore(this IKeyValueStore self, IKeyValueStore fallbackStore) {
            self.SetFallbackStore(fallbackStore);
            return self;
        }

    }
}