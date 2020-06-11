using com.csutil.io;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.jsonschema {

    public static class FieldViewMapUpdateExtensions {

        /// <summary> Helps to keep a manually modified UI updated if fields or other meta information in the schema changes. 
        /// Some operations like removing or adding fields are not performed automatically but only logged instead </summary>
        public static bool UpdateFieldViews(this Dictionary<string, FieldView> oldFieldViews, Dictionary<string, FieldView> newFieldViews,
                            bool retriggerOnViewCreated = true, bool autoDeleteRemovedFields = false) {

            // First update all field views that are found both in the list of old and new views:
            oldFieldViews.CheckIntersectingFieldViewsForChanges(newFieldViews, (oldFieldView, _, newJson) => {
                UpdateNewFieldView(oldFieldView, newJson, retriggerOnViewCreated);
            });

            // Second the list of views that have to be removed from the old UI is listed (and auto deleted if desired):
            foreach (var removed in GetOutdatedFieldViewsToDelete(oldFieldViews, newFieldViews)) {
                LogFieldThatNeedsToBeRemovedFromOldUi(removed, autoDeleteRemovedFields);
            }

            // Finally its checked if there are any missing views in the old UI that have to be manually added (picked from the new UI):
            return oldFieldViews.FindNewFieldViewsAddedIn(newFieldViews);

        }

        private static void LogFieldThatNeedsToBeRemovedFromOldUi(KeyValuePair<string, FieldView> removed, bool autoDeleteRemovedFields) {
            if (autoDeleteRemovedFields) {
                if (removed.Value.gameObject.Destroy()) {
                    Log.e($"The field '{removed.Key}' was removed from the model and was AUTOMATICALLY deleted from the UI", removed.Value.gameObject);
                    return;
                }
            }
            Log.e($"The field '{removed.Key}' was removed from the model and HAS TO BE DELETED from the UI", removed.Value.gameObject);
        }

        public static async Task LogAnyDiffToNewGeneratedUi<T>(this Dictionary<string, FieldView> self, JsonSchemaToView generator, bool forceAlwaysDelete) {
            await LogAnyDiffToNewGeneratedUi(self, typeof(T), generator, forceAlwaysDelete);
        }

        public static async Task LogAnyDiffToNewGeneratedUi(this Dictionary<string, FieldView> self, Type modelType, JsonSchemaToView generator, bool forceAlwaysDelete) {
            GameObject generatedUi = await generator.GenerateViewFrom(modelType, keepReferenceToEditorPrefab: true);
            generatedUi.name = "Delete me";
            if (!LogAnyDiffToNewFieldViews(self, generatedUi.GetFieldViewMap()) || forceAlwaysDelete) {
                generatedUi.Destroy(); // If there are no additions in the new UI it can be destroyed right away again after logging is done
            }
        }

        public static bool LogAnyDiffToNewFieldViews(this Dictionary<string, FieldView> self, Dictionary<string, FieldView> newFieldViews) {
            // First compare all field views that are found both in the list of old and new views and print out the changes:
            self.CheckIntersectingFieldViewsForChanges(newFieldViews, (oldFieldView, newFieldView, _) => {
                AssertV2.IsNotNull(oldFieldView.field, "oldFieldView.field");
                AssertV2.IsNotNull(newFieldView.field, "newFieldView.field");
                var diff = MergeJson.GetDiff(oldFieldView.field, newFieldView.field);
                if (!diff.IsNullOrEmpty()) {
                    Log.e($"Detected changed field view '{oldFieldView.fullPath}' that needs UI update! " +
                        $"Detected changes: {diff.ToPrettyString()}", oldFieldView.gameObject);
                }
            });

            // Second the list of views that have to be removed from the old UI is listed (and auto deleted if desired):
            foreach (var removed in self.GetOutdatedFieldViewsToDelete(newFieldViews)) {
                Log.e($"The field '{removed.Key}' was removed from the model and HAS TO BE DELETED from the UI", removed.Value.gameObject);
            }

            // Finally check if there are any missing views in the old UI that have to be manually added (picked from the new UI):
            return self.FindNewFieldViewsAddedIn(newFieldViews);
        }

        /// <summary> Returns a list of field views that is not present any more in the newly generated UI and has to be removed from the old UI </summary>
        public static IDictionary<string, FieldView> GetOutdatedFieldViewsToDelete(this Dictionary<string, FieldView> self, Dictionary<string, FieldView> newFieldViews) {
            return self.ExceptKeys(newFieldViews);
        }

        /// <summary> Checks if there are any missing views in the old UI that have to be manually added (picked from the new UI) </summary>
        public static bool FindNewFieldViewsAddedIn(this Dictionary<string, FieldView> self, Dictionary<string, FieldView> newFieldViews) {
            var added = newFieldViews.ExceptKeys(self);
            if (added.IsNullOrEmpty()) { return false; }
            foreach (var a in added) { Log.e($"The field '{a.Key}' was added to the model and HAS TO BE ADDED to the old UI", a.Value.gameObject); }
            return true;
        }

        public static void CheckIntersectingFieldViewsForChanges(this Dictionary<string, FieldView> self, Dictionary<string, FieldView> newFieldViews,
                        Action<FieldView, FieldView, string> onUpdateNeeded) {

            foreach (var item in self.IntersectKeys(newFieldViews)) {
                var oldFieldView = item.Value;
                var newFieldView = newFieldViews[item.Key];
                var newFieldValue = JsonWriter.GetWriter().Write(newFieldView.field);
                if (oldFieldView.fieldAsJson != newFieldValue) { onUpdateNeeded(oldFieldView, newFieldView, newFieldValue); }
                AssertV2.AreEqual(item.Value.fieldName, newFieldView.fieldName);
            }

        }

        private static void UpdateNewFieldView(FieldView oldFieldView, string newFieldValue, bool retriggerOnViewCreated) {
            StartModificationOf(oldFieldView);
            oldFieldView.fieldAsJson = newFieldValue;
            oldFieldView.OnAfterDeserialize();
            if (retriggerOnViewCreated) { oldFieldView.OnViewCreated(oldFieldView.fieldName, oldFieldView.fullPath); }
            MarkPrefabAsModified(oldFieldView);
        }

        private static void StartModificationOf(UnityEngine.Object unityObjToModify) {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(unityObjToModify, "Modify view " + unityObjToModify.name);
#endif
        }

        private static void MarkPrefabAsModified(UnityEngine.Object modifiedUnityObj) {
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(modifiedUnityObj);
#endif
        }

    }

}