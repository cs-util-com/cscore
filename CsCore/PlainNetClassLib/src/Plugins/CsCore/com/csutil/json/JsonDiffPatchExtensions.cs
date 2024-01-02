using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public static class JsonDiffPatchExtensions {

        public static JToken DiffV2(this JsonDiffPatch self, JToken left, JToken right) {
            var diff = self.Diff(left, right);
            // If the diff is not empty do a cleanup to remove all empty arrays and objects:
            if (diff != null && diff.HasValues) { CleanUp(diff); }
            return diff;
        }

        public static void CleanUp(JToken diff) {
            if (diff is JArray array) {
                for (int i = array.Count - 1; i >= 0; i--) {
                    var entry = array[i];
                    if (entry is JProperty property) {
                        CleanupProperty(property);
                    } else {
                        CleanUp(entry);
                    }
                }
            } else if (diff is JObject obj) {
                // Create a copy of the properties to iterate over
                var properties = obj.Properties().ToList();
                foreach (var property in properties) {
                    if (ShouldRemoveProperty(property) || IsEmptyJObject(property.Value)) {
                        property.Remove();
                    } else {
                        CleanUp(property.Value);
                    }
                }

                // Remove the parent property if the JObject is empty after cleanup
                if (!obj.HasValues && obj.Parent is JProperty parentProp) {
                    parentProp.Remove();
                }
            }
        }

        private static bool ShouldRemoveProperty(JProperty p) {
            return p.Value is JArray arr && arr.Count == 2 && JToken.DeepEquals(arr[0], arr[1]);
        }

        private static bool IsEmptyJObject(JToken token) {
            return token is JObject o && !o.HasValues;
        }

        private static void CleanupProperty(JProperty p) {
            if (p.Value is JArray arr && arr.Count == 2 && JToken.DeepEquals(arr[0], arr[1])) {
                p.Remove();
            } else {
                CleanUp(p.Value);
            }
        }

    }

}