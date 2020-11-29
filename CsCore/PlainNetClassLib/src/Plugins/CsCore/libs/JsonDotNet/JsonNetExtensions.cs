using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json {

    public static class JsonNetExtensions {
        public static T GetField<T>(this JObject self, string fieldName, T defaultValue) {
            if (!self.ContainsKey(fieldName)) { return defaultValue; }
            return self[fieldName].ToObject<T>();
        }

    }

}