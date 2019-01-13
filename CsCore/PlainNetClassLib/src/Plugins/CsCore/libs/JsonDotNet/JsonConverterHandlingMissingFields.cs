using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.csutil.json {

    public interface HandleAdditionalJsonFields {
        Dictionary<string, object> GetMissingFields();
        void SetMissingFields(Dictionary<string, object> missingFields);
    }
    // public void xxx() {
    //     var missingFields = (value as HandleAdditionalJsonFields).GetMissingFields();
    //     if (missingFields.IsNullOrEmpty()) { return; }
    //     foreach (var f in missingFields) { o.Add(new JProperty(f.Key, f.Value)); }
    // }

    public class JsonConverterHandlingMissingFields : JsonConverter {
        public override bool CanConvert(Type objectType) { return objectType.IsCastableTo<HandleAdditionalJsonFields>(); }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            existingValue = existingValue ?? Activator.CreateInstance(objectType, true);
            var jObject = JObject.Load(reader);
            FillFieldsOfType(objectType, existingValue, jObject);
            FillPropertiesOfType(objectType, existingValue, jObject);
            return existingValue;
        }

        private static void FillFieldsOfType(Type objectType, object existingValue, JObject jObject) {
            var fields = objectType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields) {
                var jToken = jObject[field.Name];
                if (jToken == null) { continue; }
                var value = jToken.ToObject(field.FieldType);
                field.SetValue(existingValue, value);
            }
        }

        private static void FillPropertiesOfType(Type objectType, object existingValue, JObject jObject) {
            var properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties) {
                var jToken = jObject[property.Name];
                if (jToken == null) { continue; }
                var value = jToken.ToObject(property.PropertyType);
                property.SetValue(existingValue, value, null);
            }
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer) {
            JToken t = JToken.FromObject(value);
            if (t.Type != JTokenType.Object) {
                t.WriteTo(writer);
            } else {
                JObject o = (JObject)t;
                // from https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm :
                // IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();
                // o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));
                o.WriteTo(writer);
            }
        }

    }

    // from https://stackoverflow.com/a/30365363/165106
    public class JsonConverterHandlingMissingFieldsV1 : JsonConverter {
        public bool ReportDefinedNullTokens { get; set; }

        public List<PropertyInfo> NullProperties = new List<PropertyInfo>();

        public override bool CanConvert(Type objectType) { return objectType.IsCastableTo<HandleAdditionalJsonFields>(); }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            existingValue = existingValue ?? Activator.CreateInstance(objectType, true);
            var jObject = JObject.Load(reader);
            var properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var property in properties) {
                var jToken = jObject[property.Name];
                if (jToken == null) {
                    NullProperties.Add(property);
                    continue;
                }
                var value = jToken.ToObject(property.PropertyType);
                if (ReportDefinedNullTokens && value == null) { NullProperties.Add(property); }
                property.SetValue(existingValue, value, null);
            }
            return existingValue;
        }

        //NOTE: we can omit writer part if we only want to use the converter for deserializing
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer) {
            var objectType = value.GetType();
            var properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            writer.WriteStartObject();
            foreach (var property in properties) {
                var propertyValue = property.GetValue(value, null);
                writer.WritePropertyName(property.Name);
                serializer.Serialize(writer, propertyValue);
            }
            writer.WriteEndObject();
        }
    }

    // from https://stackoverflow.com/a/29025435/165106
    class JsonConverterHandlingMissingFieldsV2 : JsonConverter {

        enum FieldDeserializationStatus { WasNotPresent, WasSetToNull, HasValue }
        interface IHasFieldStatus { Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; } }

        public override bool CanConvert(Type objectType) {
            return (objectType.IsClass && objectType.GetInterfaces().Any(i => i == typeof(IHasFieldStatus)));
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jsonObj = JObject.Load(reader);
            var targetObj = (IHasFieldStatus)Activator.CreateInstance(objectType);

            var dict = new Dictionary<string, FieldDeserializationStatus>();
            targetObj.FieldStatus = dict;

            foreach (PropertyInfo prop in objectType.GetProperties()) {
                if (prop.CanWrite && prop.Name != "FieldStatus") {
                    JToken value;
                    if (jsonObj.TryGetValue(prop.Name, StringComparison.OrdinalIgnoreCase, out value)) {
                        if (value.Type == JTokenType.Null) {
                            dict.Add(prop.Name, FieldDeserializationStatus.WasSetToNull);
                        } else {
                            prop.SetValue(targetObj, value.ToObject(prop.PropertyType, serializer));
                            dict.Add(prop.Name, FieldDeserializationStatus.HasValue);
                        }
                    } else {
                        dict.Add(prop.Name, FieldDeserializationStatus.WasNotPresent);
                    }
                }
            }
            return targetObj;
        }

        public override bool CanWrite { get { return false; } }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }

}