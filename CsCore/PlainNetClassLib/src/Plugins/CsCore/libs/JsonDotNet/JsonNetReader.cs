using System;
using System.IO;
using com.csutil;
using Newtonsoft.Json;

namespace com.csutil.json {
    public class JsonNetReader : IJsonReader {
        private IJsonWriter debugWriter = new JsonNetWriter();
        private Newtonsoft.Json.JsonSerializer reader = Newtonsoft.Json.JsonSerializer.Create(JsonNetSettings.defaultSettings);

        public T Read<T>(string jsonString) {
            var r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString, JsonNetSettings.defaultSettings);
            this.assertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString, r);
            if (r is JsonReaderFinished) { ((JsonReaderFinished)r).onJsonReadingFinished(jsonString); }
            return r;
        }

        public T Read<T>(StreamReader jsonString) {
            var r = (T)reader.Deserialize(jsonString, typeof(T));
            this.assertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString.ReadToEnd(), r);
            return r;
        }

    }
}