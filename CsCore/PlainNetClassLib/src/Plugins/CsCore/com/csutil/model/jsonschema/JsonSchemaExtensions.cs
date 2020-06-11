using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace com.csutil.model.jsonschema {

    public static class JsonSchemaExtensions {

        public static JTokenType GetJTokenType(this JsonSchema self) {
            return EnumUtil.Parse<JTokenType>(self.type.ToFirstCharUpperCase());
        }

        public static string ToJsonSchemaType(this JTokenType self) {
            return ("" + self).ToFirstCharLowerCase();
        }

        public static JToken NewDefaultJInstance(this JsonSchema self) {
            if (self.GetJTokenType() == JTokenType.Object) { return NewDefaultJObject(self); }
            return self.ParseToJValue(self.defaultVal);
        }

        private static JToken NewDefaultJObject(JsonSchema self) {
            JObject jObject = new JObject();
            foreach (var p in self.properties) { jObject.Add(p.Key, p.Value.NewDefaultJInstance()); }
            return jObject;
        }

        public static JValue ParseToJValue(this JsonSchema self, string newVal) {
            switch (self.GetJTokenType()) {
                case JTokenType.Boolean:
                    if (newVal == null) { return new JValue(false); }
                    return new JValue(bool.Parse(newVal));
                case JTokenType.Integer:
                    if (newVal == null) { return new JValue(0); }
                    return new JValue(int.Parse(newVal));
                case JTokenType.Float:
                    if (newVal == null) { return new JValue(0f); }
                    return new JValue(float.Parse(newVal));
                case JTokenType.String:
                    if (newVal == null) { return new JValue(""); }
                    return new JValue(newVal);
            }
            throw new NotImplementedException("Cant handle type " + self.type);
        }

        public static IEnumerable<string> GetOrder(this JsonSchema self) { return self.properties.Map(x => x.Key); }

    }

}