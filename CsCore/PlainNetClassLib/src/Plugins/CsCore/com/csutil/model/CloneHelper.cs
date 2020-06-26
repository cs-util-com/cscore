using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using com.csutil.json;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace com.csutil {

    public static class Mapper {

        public static T Map<T>(object source) { return CloneHelper.MapViaJsonInto<T>(source); }

    }

    public static class CloneHelper {

        public static T DeepCopy<T>(this T objectToDeepCopy, Action<T> onCopy = null) {
            var copy = objectToDeepCopy.DeepCopyViaJson();
            onCopy?.Invoke(copy);
            return copy;
        }

        public static T DeepCopyViaTypedJson<T>(this T objectToDeepCopy) {
            if (objectToDeepCopy == null) { return objectToDeepCopy; }
            var json = TypedJsonHelper.NewTypedJsonWriter().Write(objectToDeepCopy);
            return TypedJsonHelper.NewTypedJsonReader().Read<T>(json);
        }

        public static T DeepCopyViaJson<T>(this T objectToDeepCopy) {
            if (objectToDeepCopy == null) { return objectToDeepCopy; }
            return (T)MapViaJsonInto(objectToDeepCopy, objectToDeepCopy.GetType());
        }

        /// <summary> Takes an object ob type 1 and maps all its field into a type 2 using a deep copy of its JSON </summary>
        public static T MapViaJsonInto<T>(this object objectToDeepCopy) { return (T)MapViaJsonInto(objectToDeepCopy, typeof(T)); }

        private static object MapViaJsonInto(object objectToDeepCopy, Type targetType) {
            AssertV2.IsFalse(objectToDeepCopy is Task, "Tried to serialize a Task, missing (await ..)?!");
            if (objectToDeepCopy == null || objectToDeepCopy.GetType().IsPrimitiveOrSimple()) { return objectToDeepCopy; }
            using (var stream = new MemoryStream()) {
                using (var writer = new StreamWriter(stream, encoding: Encoding.UTF8, bufferSize: 1024, leaveOpen: true)) {
                    var settings = json.JsonNetSettings.defaultSettings;
                    settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    var serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(writer, objectToDeepCopy);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(stream)) {
                        using (var jtr = new JsonTextReader(sr)) {
                            return serializer.Deserialize(jtr, targetType);
                        }
                    }
                }
            }
        }

        public static T DeepCopyViaJsonString<T>(T objectToDeepCopy) {
            return DeepCopyViaJsonString(objectToDeepCopy, out string jsonString);
        }

        public static T DeepCopyViaJsonString<T>(T objectToDeepCopy, out string jsonString) {
            // ObjectCreationHandling.Replace needed so that default constructor values not added to result
            var settings = json.JsonNetSettings.defaultSettings;
            jsonString = ToJsonString(objectToDeepCopy, settings);
            // E.g: If in default constructor a list property is initialized, but in 'objectToDeepCopy' the field was set to null
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }

        private static string ToJsonString(object obj, JsonSerializerSettings s) { return JsonConvert.SerializeObject(obj, s); }

        [Obsolete("Bson logic has been deprecated in Json.net (Moved to separate package)")]
        public static T DeepCopyViaBsonStream<T>(T objectToDeepCopy) {
            using (var stream = new MemoryStream()) {
                using (var writer = new BsonWriter(stream)) {
                    var settings = json.JsonNetSettings.defaultSettings;
                    settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    var serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(writer, objectToDeepCopy);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var reader = new BsonReader(stream)) { return serializer.Deserialize<T>(reader); }
                }
            }
        }

        public static T DeepCopySerializable<T>(T objectToDeepCopy) {
            if (!typeof(T).IsSerializable) { throw new ArgumentException("The passed objectToDeepCopy must be serializable"); }
            var formatter = new BinaryFormatter();
            var stream = new MemoryStream();
            using (stream) {
                formatter.Serialize(stream, objectToDeepCopy);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static T ShallowCopyViaClone<T>(this T original) where T : ICloneable { return (T)original.Clone(); }

    }

}