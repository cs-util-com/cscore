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
            var jToken = Newtonsoft.Json.Linq.JToken.Parse(GetWriter().Write(o));
            return jToken.ToPrettyString();
        }

        public static bool HasEqualJson<T>(T a, T b) {
            if (a == null && b != null) { return false; }
            if (b == null && a != null) { return false; }
            var w = GetWriter();
            return Equals(w.Write(a), w.Write(b));
        }

    }

    public static class JTokenExtensions {

        public static string ToPrettyString(this Newtonsoft.Json.Linq.JToken jToken) {
            return jToken.ToString(Newtonsoft.Json.Formatting.Indented);
        }

    }

}