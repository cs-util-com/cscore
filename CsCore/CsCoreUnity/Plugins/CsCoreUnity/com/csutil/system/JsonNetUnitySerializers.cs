using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using UnityEngine;

namespace Newtonsoft.Json.extensions.unity {

    internal class UnityContractResolver : DefaultContractResolver {

        protected override JsonContract CreateContract(Type objectType) {
            JsonContract contract = base.CreateContract(objectType);
            if (IsVector(objectType)) { contract.Converter = new VectorConverter(); }
            return contract;
        }

        private static bool IsVector(Type t) { return t == typeof(Vector2) || t == typeof(Vector3) || t == typeof(Vector4); }

        private class VectorConverter : JsonConverter {

            public override bool CanConvert(Type objectType) { return IsVector(objectType); }

            public override object ReadJson(JsonReader reader, Type targetObjectType, object targetObjectToFill, JsonSerializer serializer) {
                targetObjectToFill = targetObjectToFill ?? Activator.CreateInstance(targetObjectType, true);
                var sourceJson = JObject.Load(reader);
                if (targetObjectToFill is Vector2 v2) {
                    v2.x = sourceJson.GetField("x", 0f);
                    v2.y = sourceJson.GetField("y", 0f);
                }
                if (targetObjectToFill is Vector3 v3) { v3.z = sourceJson.GetField("z", 0f); }
                if (targetObjectToFill is Vector4 v4) { v4.w = sourceJson.GetField("w", 0f); }
                return targetObjectToFill;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
                JObject o = new JObject();
                if (value is Vector2 v2) {
                    o.Add("x", v2.x);
                    o.Add("y", v2.y);
                }
                if (value is Vector3 v3) { o.Add("z", v3.z); }
                if (value is Vector4 v4) { o.Add("w", v4.w); }
                o.WriteTo(writer);
            }

        }

    }

}