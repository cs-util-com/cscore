using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    /// <summary> Allows the dev. to group/wrap a set of selected objects into a parent </summary>
    static class GameObjectGroupingContextMenu {

        private const string newGroupName = "Group";
        private const string undoText = "Group";

        private static bool groupingInProgress;

        [MenuItem("GameObject/Group Selected %g", true, 0)]
        static bool ValidateGroup() {
            groupingInProgress = false;
            return Selection.transforms.Length > 0;
        }

        // Original idea from https://github.com/xeleh/enhancer/blob/master/Editor/Menu/Group.cs 
        [MenuItem("GameObject/Group Selected %g", false, 0)]
        static void Group() {
            // prevent multiple execution when invoked via context menu
            if (groupingInProgress) { return; }
            // get the last top-level transform in selection
            Transform top = Selection.transforms[0];
            foreach (Transform t in Selection.transforms) {
                if (t != top && (t.parent == null || !t.IsChildOf(top.parent))) {
                    if (t.parent != top.parent || t.GetSiblingIndex() < top.GetSiblingIndex()) {
                        top = t;
                    }
                }
            }
            int siblingIndex = top.GetSiblingIndex();
            // create the group root gameobject
            GameObject groupRoot = new GameObject(newGroupName);
            Undo.RegisterCreatedObjectUndo(groupRoot, undoText);
            Selection.transforms[0].GetSiblingIndex();
            Undo.SetTransformParent(groupRoot.transform, top.parent, undoText);
            CalcOptimalPosForNewGroup(groupRoot);
            // re-parent transforms in selection
            foreach (Transform t in Selection.transforms) {
                Undo.SetTransformParent(t, groupRoot.transform, undoText);
            }
            groupRoot.transform.SetSiblingIndex(siblingIndex);
            Selection.activeGameObject = groupRoot;
            groupingInProgress = true;
        }

        /// <summary> Calculate the group root new position (average point) </summary>
        private static void CalcOptimalPosForNewGroup(GameObject groupRoot) {
            Vector3 averagePoint = Vector3.zero;
            bool zeroCenterFound = false;
            foreach (Transform t in Selection.transforms) {
                if (t.position == Vector3.zero) { zeroCenterFound = true; }
                averagePoint += t.position;
            }
            averagePoint /= Selection.transforms.Length;
            if (!zeroCenterFound) { groupRoot.transform.position = averagePoint; }
        }

    }

}