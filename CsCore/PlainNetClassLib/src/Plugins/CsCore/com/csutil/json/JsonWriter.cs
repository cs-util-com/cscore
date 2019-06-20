using System;
using System.IO;
using com.csutil.json;

namespace com.csutil {
    public interface IJsonWriter {
        string Write(object data);
        void Write(object data, StreamWriter streamWriter);
    }

    public static class JsonWriter {

        public static IJsonWriter GetWriter() {
            return IoC.inject.GetOrAddSingleton<IJsonWriter>(new object(), () => new JsonNetWriter());
        }

        public static string AsPrettyString(object o) {
            var jToken = Newtonsoft.Json.Linq.JToken.Parse(JsonWriter.GetWriter().Write(o));
            var prettyJsonString = jToken.ToString(Newtonsoft.Json.Formatting.Indented);
            return prettyJsonString;
        }

    }

}