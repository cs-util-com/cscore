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

        /// <summary> Gets the systems current default typed JSON writer.
        /// Typed here means that it's a writer that tries to keep class inheritance and
        /// similar non-native json features intact </summary>
        /// <param name="caller"> Typically the object being written should be passed here </param>
        public static IJsonWriterTyped GetWriterTyped(object caller) {
            return IoC.inject.GetOrAddSingleton<IJsonWriterTyped>(caller, () => new JsonNetWriterTyped());
        }

        public static string AsPrettyString(object o) {
            return GetWriter(o).AsPrettyString(o);
        }

        public static string AsPrettyString(this IJsonWriter jsonWriter, object o) {
            var jToken = Newtonsoft.Json.Linq.JToken.Parse(jsonWriter.Write(o));
            return jToken.ToPrettyString();
        }

        [Obsolete("Use myJsonWriter.HasEqualJson(..) instead", true)]
        public static bool HasEqualJson<T>(T a, T b) {
            if (a == null && b != null) { return false; }
            if (b == null && a != null) { return false; }
            return GetWriter(a).HasEqualJson(a, b);
        }

        public static bool HasEqualJson<T>(this IJsonWriter w, T a, T b) {
            if (a == null && b != null) { return false; }
            if (b == null && a != null) { return false; }
            return Equals(w.Write(a), w.Write(b));
        }

    }

    public static class JTokenExtensions {

        public static string ToPrettyString(this Newtonsoft.Json.Linq.JToken jToken) {
            if (jToken == null) { return "null"; }
            return jToken.ToString(Newtonsoft.Json.Formatting.Indented);
        }

    }

}