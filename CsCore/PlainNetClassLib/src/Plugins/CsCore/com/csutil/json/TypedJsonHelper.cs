using System;

namespace com.csutil.json {

    public interface IJsonReaderTyped : IJsonReader {
    }

    public interface IJsonWriterTyped : IJsonWriter {
    }

    public class JsonNetWriterTyped : JsonNetWriter, IJsonWriterTyped {
        public JsonNetWriterTyped() : base(JsonNetSettings.typedJsonSettings) { }
    }

    public class JsonNetReaderTyped : JsonNetReader, IJsonReaderTyped {
        public JsonNetReaderTyped(JsonNetWriter debugWriter = null) : base(JsonNetSettings.typedJsonSettings, debugWriter) { }
    }

    public static class TypedJsonHelper {

        [Obsolete("Consider using JsonWriter.GetTypedReader() instead", true)]
        public static IJsonReaderTyped NewTypedJsonReader() { return new JsonNetReaderTyped(null); }

        [Obsolete("Consider using JsonWriter.GetTypedWriter() instead", true)]
        public static IJsonWriterTyped NewTypedJsonWriter() { return new JsonNetWriterTyped(); }

        [Obsolete("Use JsonWriter.GetTypedWriter() and JsonReader.GetTypedReader() instead", true)]
        public static void SetupTypedJsonWriterAndReaderSingletons(bool overrideExisting) {
            IoC.inject.SetSingleton<IJsonWriter>(new JsonNetWriter(JsonNetSettings.typedJsonSettings), overrideExisting);
            IoC.inject.SetSingleton<IJsonReader>(new JsonNetReader(JsonNetSettings.typedJsonSettings), overrideExisting);
        }

    }

}