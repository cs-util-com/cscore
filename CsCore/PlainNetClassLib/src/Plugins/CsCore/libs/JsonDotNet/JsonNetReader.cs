using System;
using System.IO;
using com.csutil;
using Newtonsoft.Json;

namespace com.csutil.json {

    public class JsonNetReader : IJsonReader {

        private IJsonWriter debugWriter = new JsonNetWriter();
        private JsonSerializer reader = JsonSerializer.Create(JsonNetSettings.defaultSettings);

        public T Read<T>(string jsonString) {
            var r = JsonConvert.DeserializeObject<T>(jsonString, JsonNetSettings.defaultSettings);
            this.AssertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString, r);
            if (r is JsonReaderFinished) { ((JsonReaderFinished)r).onJsonReadingFinished(jsonString); }
            return r;
        }

        public T Read<T>(StreamReader jsonString) {
            var r = (T)reader.Deserialize(jsonString, typeof(T));
            this.AssertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString.ReadToEnd(), r);
            return r;
        }

    }

}