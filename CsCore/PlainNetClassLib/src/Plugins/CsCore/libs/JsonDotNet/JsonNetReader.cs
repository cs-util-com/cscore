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

        public T Read<T>(StreamReader streamReader) {
            var r = (T)reader.Deserialize(streamReader, typeof(T));
            (this).AssertThatJsonWasFullyParsedIntoFields(debugWriter, ReadFullString(streamReader), r);
            return r;
        }

        private static string ReadFullString(StreamReader streamReader) {
            streamReader.DiscardBufferedData();
            streamReader.BaseStream.Position = 0;
            var fullString = streamReader.ReadToEnd();
            AssertV2.IsFalse(fullString.IsNullOrEmpty(), "The string loaded from the streamReader was null or emtpy");
            return fullString;
        }
    }

}