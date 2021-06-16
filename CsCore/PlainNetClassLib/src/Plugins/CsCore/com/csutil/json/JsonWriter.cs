using System;
using System.IO;
using com.csutil.json;

namespace com.csutil {
    public interface IJsonWriter {
        string Write(object data);
        void Write(object data, StreamWriter streamWriter);
    }

    public static class JsonWriter {

        [Obsolete("Pass a caller (tycally the object to write should be passed here)")]
        public static IJsonWriter GetWriter() { return GetWriter(new object()); }

        /// <summary> Gets the systems current default JSON writer </summary>
        /// <param name="caller"> Typically the object being written should be passed here </param>
        public static IJsonWriter GetWriter(object caller) {
            return IoC.inject.GetOrAddSingleton<IJsonWriter>(caller, () => new JsonNetWriter());
            //if (IoC.inject.TryGet(caller, out IJsonWriter w)) { return w; }
            //var writer = new JsonNetWriter();
            //IoC.inject.RegisterInjector(caller, (_, createIfNull) => writer);
            //return writer;
        }

        public static string AsPrettyString(object o) {
            var jToken = Newtonsoft.Json.Linq.JToken.Parse(GetWriter(o).Write(o));
            return jToken.ToPrettyString();
        }

        public static bool HasEqualJson<T>(T a, T b) {
            if (a == null && b != null) { return false; }
            if (b == null && a != null) { return false; }
            var w = GetWriter(a);
            return Equals(w.Write(a), w.Write(b));
        }

    }

    public static class JTokenExtensions {

        public static string ToPrettyString(this Newtonsoft.Json.Linq.JToken jToken) {
            return jToken.ToString(Newtonsoft.Json.Formatting.Indented);
        }

    }

}