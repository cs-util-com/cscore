using System;
using System.Collections.Generic;
using System.Linq;
using DiffMatchPatch;

namespace com.csutil {

    public static class MergeText {

        public class Result {
            public object[] rawResult;
            public Dictionary<Patch, bool> patches;
            public string mergeResult;
            public override string ToString() { return mergeResult; }
        }

        public static Result Merge(string originalText, string editedText_1, string editedText_2) {
            return new diff_match_patch().Merge(originalText, editedText_1, editedText_2);
        }

        public static Result Merge(this diff_match_patch dmp, string originalText, string editedText_1, string editedText_2) {
            var res = new Result();
            var patches = dmp.patch_make(originalText, editedText_1);
            res.rawResult = dmp.patch_apply(patches, editedText_2);
            res.mergeResult = res.rawResult[0] as string;
            var patchResults = (res.rawResult[1] as IEnumerable<bool>);
            res.patches = patches.ToDictionary(patchResults);
            return res;
        }

    }

}