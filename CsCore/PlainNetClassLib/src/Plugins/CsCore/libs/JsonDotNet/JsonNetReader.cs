using System;
using System.IO;
using Newtonsoft.Json;

namespace com.csutil.json {

    public class JsonNetReader : IJsonReader {

        /// <summary> If set this will assert, that the passed json was fully parsed into the fields of the target class </summary>
        public IJsonWriter debugWriter;

        public JsonSerializerSettings settings { get; private set; }
        private JsonSerializer reader;

        public JsonNetReader() : this(JsonNetSettings.defaultSettings) { }
        public JsonNetReader(JsonSerializerSettings settings) : this(settings, null) { }

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
            streamReader.BaseStream.ResetStreamCurserPositionToBeginning();
            var fullString = streamReader.ReadToEnd();
            AssertV3.IsFalse(fullString.IsNullOrEmpty(), () => "The string loaded from the streamReader was null or emtpy");
            return fullString;
        }
    }

}