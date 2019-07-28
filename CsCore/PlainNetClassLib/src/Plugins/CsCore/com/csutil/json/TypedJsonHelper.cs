namespace com.csutil.json {
    public class TypedJsonHelper {

        public static IJsonReader NewTypedJsonReader() { return new JsonNetReader(JsonNetSettings.typedJsonSettings, null); }

        public static IJsonWriter NewTypedJsonWriter() { return new JsonNetWriter(JsonNetSettings.typedJsonSettings); }

    }

}