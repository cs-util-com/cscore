using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace com.csutil {

    public static class Mapper {

        public static T Map<T>(object source) { return CloneHelper.MapViaJsonInto<T>(source); }

    }

    public static class CloneHelper {

        public static T DeepCopyViaJson<T>(this T objectToDeepCopy) { return MapViaJsonInto<T>(objectToDeepCopy); }

        /// <summary> Takes an object ob type 1 and maps all its field into a type 2 using a deep copy of its JSON </summary>
        public static T MapViaJsonInto<T>(this object objectToDeepCopy) {
            using (var stream = new MemoryStream()) {
                using (var writer = new StreamWriter(stream, encoding: Encoding.UTF8, bufferSize: 1024, leaveOpen: true)) {
                    var settings = json.JsonNetSettings.defaultSettings;
                    settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    var serializer = JsonSerializer.Create(settings);
                    serializer.Serialize(writer, objectToDeepCopy);
                    writer.Flush();
                    stream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(stream)) { using (var jtr = new JsonTextReader(sr)) { return serializer.Deserialize<T>(jtr); } }
                }
            }
        }

        public static T DeepCopyViaJsonString<T>(T objectToDeepCopy) {
            return DeepCopyViaJsonString(objectToDeepCopy, out string jsonString);
        }

        public static T DeepCopyViaJsonString<T>(T objectToDeepCopy, out string jsonString) {
            // ObjectCreationHandling.Replace needed so that default constructor values not added to result
            jsonString = JsonConvert.SerializeObject(objectToDeepCopy);
            var settings = json.JsonNetSettings.defaultSettings;
            // E.g: If in default constructor a list property is initialized, but in 'objectToDeepCopy' the field was set to null
            settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }

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