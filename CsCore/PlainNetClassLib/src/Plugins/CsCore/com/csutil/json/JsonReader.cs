using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.csutil.json;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public interface IJsonReader {
        object ReadAsType(string jsonString, Type targetType);
        object ReadAsType(StreamReader streamReader, Type targetType);
        T Read<T>(string jsonString);
        T Read<T>(StreamReader jsonString);
    }

    public interface JsonReaderFinished {
        void onJsonReadingFinished(string originalRawJson);
    }

    public static class JsonReader {

        [Obsolete("Use version where caller has to be provided")]
        public static IJsonReader GetReader() { return GetReader(null); }

        public static IJsonReader GetReader(object caller) {
            return IoC.inject.GetOrAddSingleton<IJsonReader>(caller, () => new JsonNetReader());
            //if (IoC.inject.TryGet(caller, out IJsonReader r)) { return r; }
            //var reader = new JsonNetReader();
            //IoC.inject.RegisterInjector(caller, (_, createIfNull) => reader);
            //return reader;
        }
        
        public static IJsonReaderTyped GetReaderTyped(object caller) {
            return IoC.inject.GetOrAddSingleton<IJsonReaderTyped>(caller, () => new JsonNetReaderTyped());
        }

        /// <summary> this method makes sure that classes provided by the internal json parsing libs are converted 
        /// to generic classes like Dictionary<string, object> or Dictionary<string, object>[] </summary>
        public static object ConvertToGenericDictionaryOrArray(object value) {
            if (value is JObject) { return jsonNetObjectToDictionary((JObject)value); }
            if (value is JArray) { return jsonNetArraytoArrayOfDict((JArray)value); }
            return value;
        }

        private static object jsonNetArraytoArrayOfDict(JArray value) {
            try { return value.Map(x => ConvertToGenericDictionaryOrArray(x)).ToArray(); } catch (Exception e) { Log.w("" + e); }
            return value;
        }
        private static Dictionary<string, object> jsonNetObjectToDictionary(JObject c) { return c.ToObject<Dictionary<string, object>>(); }


    }

}