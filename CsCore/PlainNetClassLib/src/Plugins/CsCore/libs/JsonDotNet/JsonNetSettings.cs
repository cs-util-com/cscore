using Newtonsoft.Json;

namespace com.csutil.json {

    public static class JsonNetSettings {

        public static JsonSerializerSettings defaultSettings = newJsonNetSettings();
        private static JsonSerializerSettings newJsonNetSettings() {
            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            return settings;
        }

    }

}