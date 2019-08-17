using System;
using System.Collections.Generic;
using System.Linq;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    public static class MergeJson {

        public static Result<T> Merge<T>(T originalObj, T variant1, T variant2) {
            return new JsonDiffPatch().Merge(originalObj, variant1, variant2);
        }

        public static Result<T> Merge<T>(this JsonDiffPatch self, T originalObj, T variant1, T variant2) {
            var res = new Result<T>();
            res.original = JToken.FromObject(originalObj);
            res.variant1 = JToken.FromObject(variant1);
            res.variant2 = JToken.FromObject(variant2);
            res.patch2 = self.Diff(res.original, res.variant2);
            res.mergeOf2Into1 = self.Patch(res.variant1, res.patch2);
            res.patch1 = self.Diff(res.original, res.variant1);
            res.mergeOf1Into2 = self.Patch(res.variant2, res.patch1);
            if (!res.mergeOf2Into1.Equals(res.mergeOf1Into2)) {
                res.conflicts = self.Diff(res.mergeOf1Into2, res.mergeOf2Into1);
            }
            return res;
        }

        public class Result<T> {
            internal JToken original;
            internal JToken variant1;
            internal JToken variant2;
            internal JToken patch2;
            internal JToken mergeOf2Into1;
            internal JToken patch1;
            internal JToken mergeOf1Into2;
            internal JToken conflicts;

            public T result { get { return mergeOf2Into1.ToObject<T>(); } }

            public bool hasMergeConflict { get { return !conflicts.IsNullOrEmpty(); } }

            public IEnumerable<MergeConflict> mergeConflicts {
                get { return conflicts.Map<JToken, MergeConflict>(ParseIntoConflict); }
            }

            private MergeConflict ParseIntoConflict(JToken t) {
                var res = new MergeConflict() { };
                var prop = t as JProperty;
                res.fieldName = prop.Name;
                var oldAndNewValue = prop.Value as JArray;
                AssertV2.AreEqual(2, oldAndNewValue.Count);
                res.oldValue = oldAndNewValue[0];
                res.newValue = oldAndNewValue[1];
                return res;
            }

        }

        public class MergeConflict {
            public string fieldName;
            public JToken oldValue;
            public JToken newValue;
        }

    }

}