using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using JsonDiffPatchDotNet;
using Newtonsoft.Json.Linq;

namespace com.csutil {

    /// <summary> Can be used to persist a state to allow asserting it will not change.  </summary>
    public class PersistedRegression {

        private const string DEFAULT_FOLDER_NAME = "RegressionTesting";

        public IKeyValueStore regressionStore;

        public PersistedRegression(IKeyValueStore regressionStore = null) {
            if (regressionStore == null) { regressionStore = new FileBasedKeyValueStore(GetProjDir()); }
            this.regressionStore = regressionStore;
        }

        private static DirectoryInfo GetProjDir() {
            var binDir = EnvironmentV2.instance.GetCurrentDirectory();
            var projDir = SearchForVersionedParent(binDir);
            if (projDir == null) { projDir = binDir; }
            return projDir.GetChildDir(DEFAULT_FOLDER_NAME);
        }

        private static DirectoryInfo SearchForVersionedParent(DirectoryInfo f) {
            if (f == null || f.GetChild(".gitignore").ExistsV2()) { return f; }
            return SearchForVersionedParent(f.Parent);
        }

        public async Task<JToken> VerifyState(string id, params object[] objectsToCheck) {
            if (!await regressionStore.ContainsKey(id)) {
                await SaveToRegressionStore(id, objectsToCheck);
                return null;
            } else {
                var jsonString = await regressionStore.Get<string>(id, null);
                var oldJson = JToken.Parse(jsonString);
                var newJson = JToken.Parse(JsonWriter.GetWriter().Write(objectsToCheck));
                var diff = new JsonDiffPatch().Diff(oldJson, newJson);
                return diff;
            }
        }

        private async Task SaveToRegressionStore(string id, object[] objectsToCheck) {
            var jToken = JToken.Parse(JsonWriter.GetWriter().Write(objectsToCheck));
            await regressionStore.Set(id, jToken.ToPrettyString());
        }

        public Task AssertState(string id, params object[] objectsToCheck) {
            return AssertState(id, p => true, objectsToCheck);
        }

        public async Task AssertState(string id, Func<JToken, bool> filterAcceptableDiffs, params object[] objectsToCheck) {
            JToken diffToExpectedState = await VerifyState(id, objectsToCheck);
            if (!diffToExpectedState.IsNullOrEmpty()) {
                var foundProblems = GetFilteredDiff(diffToExpectedState, filterAcceptableDiffs);
                if (!foundProblems.IsNullOrEmpty()) {
                    var problemReport = foundProblems.ToStringV2(p => p.ToPrettyString());
                    throw new Exception("Diff found in regression test: " + problemReport);
                }
            }
        }

        private static IEnumerable<JToken> GetFilteredDiff(JToken diffToExpectedState, Func<JToken, bool> filterAcceptableDiffs) {
            return diffToExpectedState.Children().Skip(1).Filter(problem => {
                AssertV2.AreEqual(1, problem.Count(), "The diff contained not exactly one element");
                return filterAcceptableDiffs(problem.First());
            });
        }

    }

}