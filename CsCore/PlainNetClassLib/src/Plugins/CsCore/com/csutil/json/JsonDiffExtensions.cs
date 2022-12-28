using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace com.csutil.json {

    public static class JsonDiffExtensions {

        public static bool HasDiffIn(this JToken self, string propertyName, Func<JToken, JToken, bool> matchDiffDetails) {
            if (self is JProperty p && p.Name == propertyName) {
                if (p.Value is JArray a) {
                    if (a.Count() == 2 && matchDiffDetails(a.First(), a.Last())) {
                        return true;
                    }
                }
            }
            return false;
        }

    }

}