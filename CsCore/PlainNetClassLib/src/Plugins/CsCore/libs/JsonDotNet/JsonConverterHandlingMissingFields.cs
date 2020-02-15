using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.csutil.json {

    /// <summary> Allows to keep fields that are unknown to the class the json is parsed into </summary>
    internal class JsonConverterHandlingMissingFields : JsonConverter {

        // This converter should only handle classes that implement the HandleAdditionalJsonFields interface:
        public override bool CanConvert(Type objectType) {
            return objectType.IsCastableTo<HandleAdditionalJsonFields>();
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type targetObjectType, object targetObjectToFill, JsonSerializer serializer) {
            targetObjectToFill = targetObjectToFill ?? Activator.CreateInstance(targetObjectType, true);
            AssertV2.IsTrue(targetObjectToFill is HandleAdditionalJsonFields, "targetObjectToFill did not implement HandleAdditionalJsonFields");
            var sourceJson = JObject.Load(reader);
            var missingFieldList = FillTargetObjectAndCollectMissingFields(targetObjectType, targetObjectToFill, sourceJson);
            if (!missingFieldList.IsNullOrEmpty()) {
                var missingFields = missingFieldList.ToDictionary(x => x.Key, x => (object)x.Value);
                (targetObjectToFill as HandleAdditionalJsonFields).SetAdditionalJsonFields(missingFields);
            }
            return targetObjectToFill;
        }

        private static List<KeyValuePair<string, JToken>> FillTargetObjectAndCollectMissingFields(Type targetObjectType, object targetObjectToFill, JObject sourceJson) {
            var fieldsMissingInTargetObjectType = new List<KeyValuePair<string, JToken>>();
            foreach (var jsonField in sourceJson) {
                // First search for a field of this name:
                var field = targetObjectType.GetField(jsonField.Key);
                if (field != null) { field.SetValue(targetObjectToFill, jsonField.Value.ToObject(field.FieldType)); continue; }
                // Then search for a property of this name:
                var property = targetObjectType.GetProperty(jsonField.Key);
                if (property != null) { property.SetValue(targetObjectToFill, jsonField.Value.ToObject(property.PropertyType), null); continue; }
                // Then add it to the list of lissing fields:
                fieldsMissingInTargetObjectType.Add(jsonField);
            }
            return fieldsMissingInTargetObjectType;
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, JsonSerializer serializer) {
            // See also reference example at https://www.newtonsoft.com/json/help/html/CustomJsonConverter.htm
            JToken t = JToken.FromObject(value);
            if (t is JObject) {
                JObject o = (JObject)t;
                var missingFields = (value as HandleAdditionalJsonFields).GetAdditionalJsonFields();
                if (!missingFields.IsNullOrEmpty()) {
                    foreach (var f in missingFields) { o.Add(new JProperty(f.Key, f.Value)); }
                }
            }
            t.WriteTo(writer);
        }

    }

}