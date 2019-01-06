using System.IO;
using Newtonsoft.Json;

namespace com.csutil.json {

    public class JsonNetWriter : IJsonWriter {

        private JsonSerializer writer = JsonSerializer.Create(JsonNetSettings.defaultSettings);
        public string Write(object data) { return JsonConvert.SerializeObject(data, JsonNetSettings.defaultSettings); }
        public void Write(object data, StreamWriter streamWriter) { writer.Serialize(streamWriter, data); }

    }

}