using System;
using Newtonsoft.Json;

namespace com.csutil.json {

    public static class JsonNetSettings {

        public static JsonSerializerSettings defaultSettings = newDefaultSettings();

        private static JsonSerializerSettings newDefaultSettings() {
            var s = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore, };
            s.Converters.Add(new JsonConverterHandlingMissingFields());
            return s;
        }
        
    }

}