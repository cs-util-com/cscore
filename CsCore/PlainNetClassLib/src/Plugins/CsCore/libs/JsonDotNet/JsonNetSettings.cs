using Newtonsoft.Json;

namespace com.csutil.json {

    public static class JsonNetSettings {

        public static JsonSerializerSettings defaultSettings => NewDefaultSettings();
        public static JsonSerializerSettings typedJsonSettings => NewTypedJsonSettings();

        private static JsonSerializerSettings NewDefaultSettings() {
            var s = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, };
            s.Converters.Add(new JsonConverterHandlingMissingFields());
            return s;
        }

        private static JsonSerializerSettings NewTypedJsonSettings() {
            var settings = NewDefaultSettings();
            settings.TypeNameHandling = TypeNameHandling.Auto;
            return settings;
        }

    }

}