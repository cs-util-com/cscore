using Newtonsoft.Json;

namespace com.csutil.json {

    public static class JsonNetSettings {

        public static JsonSerializerSettings defaultSettings = NewDefaultSettings();
        public static JsonSerializerSettings typedJsonSettings = NewTypedJsonSettings();

        public static JsonSerializerSettings NewDefaultSettings() {
            var s = new JsonSerializerSettings();
            s.Converters.Add(new JsonConverterHandlingMissingFields());
            return s;
        }

        public static JsonSerializerSettings NewTypedJsonSettings() {
            var settings = NewDefaultSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            return settings;
        }

    }

}