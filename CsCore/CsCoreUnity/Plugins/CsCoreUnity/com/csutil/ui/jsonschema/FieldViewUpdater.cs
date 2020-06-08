using com.csutil.model.jsonschema;
using UnityEngine;

namespace com.csutil.ui.jsonschema {

    /// <summary> Helps to keep a manually modified UI updated if fields or other meta information in the schema changes. 
    /// Some operations like removing or adding fields are not performed automatically but only logged instead </summary>
    public static class FieldViewUpdater {

        public static void UpdateFieldViews(GameObject targetView, GameObject newGeneratedView, bool retriggerOnViewCreated = true) {
            var oldFieldViews = targetView.GetFieldViewMap();
            var newFieldViews = newGeneratedView.GetFieldViewMap();
            foreach (var item in oldFieldViews.IntersectKeys(newFieldViews)) {
                var oldFieldView = item.Value;
                var newFieldView = newFieldViews[item.Key];
                var newFieldValue = JsonWriter.GetWriter().Write(newFieldView.field);
                if (oldFieldView.fieldAsJson != newFieldValue) {
                    StartModificationOf(oldFieldView);
                    oldFieldView.fieldAsJson = newFieldValue;
                    oldFieldView.field = JsonReader.GetReader().Read<JsonSchema>(newFieldValue);
                    if (retriggerOnViewCreated) {
                        oldFieldView.OnViewCreated(oldFieldView.fieldName, oldFieldView.fullPath);
                    }
                }
                AssertV2.AreEqual(item.Value.fieldName, newFieldView.fieldName);
                MarkPrefabAsModified(oldFieldView);
            }

            foreach (var missing in newFieldViews.ExceptKeys(oldFieldViews)) {
                Log.e($"The field {missing.Key} was added to the model and has to be added to the UI", missing.Value.gameObject);
            }

            foreach (var removed in oldFieldViews.ExceptKeys(newFieldViews)) {
                Log.e($"The field {removed.Key} was removed from the model and has to be deleted from the UI", removed.Value.gameObject);
            }
        }

        private static void StartModificationOf(Object unityObjToModify) {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(unityObjToModify, "Modify view " + unityObjToModify.name);
#endif
        }

        private static void MarkPrefabAsModified(Object modifiedUnityObj) {
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(modifiedUnityObj);
#endif
        }

    }

}