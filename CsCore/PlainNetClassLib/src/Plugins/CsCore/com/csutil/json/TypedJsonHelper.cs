namespace com.csutil.json {

    public static class TypedJsonHelper {

        public static IJsonReader NewTypedJsonReader() { return new JsonNetReader(JsonNetSettings.typedJsonSettings, null); }

        public static IJsonWriter NewTypedJsonWriter() { return new JsonNetWriter(JsonNetSettings.typedJsonSettings); }

        public static void SetupTypedJsonWriterAndReaderSingletons(bool overrideExisting) {
            IoC.inject.SetSingleton<IJsonWriter>(new JsonNetWriter(JsonNetSettings.typedJsonSettings), overrideExisting);
            IoC.inject.SetSingleton<IJsonReader>(new JsonNetReader(JsonNetSettings.typedJsonSettings), overrideExisting);
        }
        
    }

}