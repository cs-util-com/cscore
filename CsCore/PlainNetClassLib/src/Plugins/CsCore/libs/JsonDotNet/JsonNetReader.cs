using System;
using System.IO;
using com.csutil;
using Newtonsoft.Json;

namespace com.csutil.json {

    public class JsonNetReader : IJsonReader {

        private IJsonWriter debugWriter;
        private JsonSerializerSettings settings;
        private JsonSerializer reader;

        public JsonNetReader() : this(JsonNetSettings.defaultSettings) { }
        public JsonNetReader(JsonSerializerSettings settings) : this(settings, new JsonNetWriter(settings)) { }

        public JsonNetReader(JsonSerializerSettings settings, JsonNetWriter debugWriter) {
            this.settings = settings;
            this.reader = JsonSerializer.Create(settings);
            this.debugWriter = debugWriter;
        }

        public object ReadAsType(string jsonString, Type targetType) {
            var r = JsonConvert.DeserializeObject(jsonString, targetType, settings);
            this.AssertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString, r);
            if (r is JsonReaderFinished) { ((JsonReaderFinished)r).onJsonReadingFinished(jsonString); }
            return r;
        }

        public object ReadAsType(StreamReader streamReader, Type targetType) {
            var r = reader.Deserialize(streamReader, targetType);
            this.AssertThatJsonWasFullyParsedIntoFields(debugWriter, ReadFullString(streamReader), r);
            return r;
        }

        public T Read<T>(string jsonString) {
            var r = JsonConvert.DeserializeObject<T>(jsonString, settings);
            this.AssertThatJsonWasFullyParsedIntoFields(debugWriter, jsonString, r);
            if (r is JsonReaderFinished) { ((JsonReaderFinished)r).onJsonReadingFinished(jsonString); }
            return r;
        }

        public T Read<T>(StreamReader streamReader) {
            var r = (T)reader.Deserialize(streamReader, typeof(T));
            //(this).AssertThatJsonWasFullyParsedIntoFields(debugWriter, ReadFullString(streamReader), r);
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