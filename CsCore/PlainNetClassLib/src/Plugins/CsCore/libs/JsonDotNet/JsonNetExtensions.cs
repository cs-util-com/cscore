using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json {

    public static class JsonNetExtensions {

        public static T GetField<T>(this JObject self, string fieldName, T defaultValue) {
            if (!self.ContainsKey(fieldName)) { return defaultValue; }
            return self[fieldName].ToObject<T>();
        }

        /// <summary> Deserializes a JToken into an existing object, overwrites the fields defined in the JToken </summary>
        public static void PopulateInto(this JToken self, object target, JsonSerializer s) {
            using (var sr = self.CreateReader()) { s.Populate(sr, target); }
        }

    }

}