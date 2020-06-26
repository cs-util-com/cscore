using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.json;
using JsonDiffPatchDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public static class MergeJson {

        public static Result<T> Merge<T>(T originalObj, T variant1, T variant2) {
            return new JsonDiffPatch().Merge(originalObj, variant1, variant2);
        }

        public static Result<T> Merge<T>(this JsonDiffPatch self, T originalObj, T variant1, T variant2) {
            var res = new Result<T>();
            var s = JsonSerializer.Create(JsonNetSettings.defaultSettings);
            res.original = JToken.FromObject(originalObj, s);
            res.variant1 = JToken.FromObject(variant1, s);
            res.variant2 = JToken.FromObject(variant2, s);
            res.patch2 = self.Diff(res.original, res.variant2);
            res.mergeOf2Into1 = self.Patch(res.variant1, res.patch2);
            res.patch1 = self.Diff(res.original, res.variant1);
            res.mergeOf1Into2 = self.Patch(res.variant2, res.patch1);
            if (!JToken.DeepEquals(res.mergeOf2Into1, res.mergeOf1Into2)) {
                res.conflicts = self.Diff(res.mergeOf1Into2, res.mergeOf2Into1);
            }
            return res;
        }

        public static JToken GetDiff<T>(T a, T b) {
            return GetDiff(a, b, JsonSerializer.Create(JsonNetSettings.defaultSettings));
        }

        public static JToken GetDiff<T>(T a, T b, JsonSerializer s) {
            return new JsonDiffPatch().Diff(JToken.FromObject(a, s), JToken.FromObject(b, s));
        }

        public class Result<T> {
            internal JToken original;
            internal JToken variant1;
            internal JToken variant2;
            internal JToken patch2;
            internal JToken mergeOf2Into1;
            internal JToken patch1;
            internal JToken mergeOf1Into2;
            public JToken conflicts;

            public T GetResult() { return mergeOf2Into1.ToObject<T>(); }

            public bool hasMergeConflict { get { return !conflicts.IsNullOrEmpty(); } }

            public IEnumerable<MergeConflict> GetParsedMergeConflicts() {
                return conflicts.Map<JToken, MergeConflict>(ParseIntoConflict);
            }

            private MergeConflict ParseIntoConflict(JToken token) {
                if (token is JProperty prop) { return ToMergeConflict(prop); }
                throw new NotImplementedException("Unhandled type " + token.GetType() + ":\n" + token.ToPrettyString());
            }

            public static MergeConflict ToMergeConflict(JProperty prop) {
                var res = new MergeConflict();
                res.fieldName = prop.Name;
                if (prop.Value is JObject o) {
                    var properties = o.Properties();
                    if (IsArray(properties.First())) {
                        res.specialType = MergeConflict.SPECIAL_TYPE_ARRAY;
                        properties = properties.Skip(1);
                    }
                    res.conflicts = properties.Map(ToMergeConflict);
                } else if (prop.Value is JArray oldAndNewValue) {
                    AssertV2.AreEqual(2, oldAndNewValue.Count);
                    res.oldValue = oldAndNewValue[0];
                    res.newValue = oldAndNewValue[1];
                } else {
                    throw new NotImplementedException("Unhandled type " + prop.Value.GetType() + ":\n" + prop.Value.ToPrettyString());
                }
                return res;
            }

            private static bool IsArray(JProperty p) { return Equals("_t", p.Name) && p.Value is JValue v && Equals("a", v.Value); }

        }

        public class MergeConflict {
            internal const string SPECIAL_TYPE_ARRAY = "array";
            public string fieldName;
            public IEnumerable<MergeConflict> conflicts;
            public JToken oldValue;
            public JToken newValue;
            public string specialType;
        }

    }

}