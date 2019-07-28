using System.IO;
using Newtonsoft.Json;

namespace com.csutil.json {

    public class JsonNetWriter : IJsonWriter {
        private JsonSerializerSettings settings;
        private JsonSerializer writer;
        public JsonNetWriter() : this(JsonNetSettings.defaultSettings) { }

        public JsonNetWriter(JsonSerializerSettings settings) {
            this.settings = settings;
            writer = JsonSerializer.Create(settings);
        }

        public string Write(object data) { return JsonConvert.SerializeObject(data, typeof(object), settings); }
        public void Write(object data, StreamWriter streamWriter) { writer.Serialize(streamWriter, data, typeof(object)); }

    }

}