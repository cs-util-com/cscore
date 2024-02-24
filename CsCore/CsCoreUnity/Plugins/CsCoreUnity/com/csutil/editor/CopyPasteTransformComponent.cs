using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    /// <summary> Modified version of https://github.com/sirgru/Unity-Simple-Editor-Shortcuts-Tools-Collection </summary>
    public static class CopyPasteTransformComponent {

        private static TransformData copiedTransformValues;

        [MenuItem("Edit/Copy Transform Values %#c", false, -101)]
        public static void CopyTransformValues() {
            if (Selection.gameObjects.Length == 0) return;
            var transformToCopy = Selection.gameObjects[0].transform;

            if (transformToCopy is RectTransform rt) {
                copiedTransformValues = new TransformData() {
                    position = transformToCopy.position,
                    rotation = transformToCopy.rotation,
                    lossyScale = transformToCopy.lossyScale,
                };
                // Additionally copy the anchors, pivot, width and height etc:
                copiedTransformValues.anchorMin = rt.anchorMin;
                copiedTransformValues.anchorMax = rt.anchorMax;
                copiedTransformValues.anchoredPosition = rt.anchoredPosition;
                copiedTransformValues.pivot = rt.pivot;
                copiedTransformValues.sizeDelta = rt.sizeDelta;
            } else {
                copiedTransformValues = new TransformData() {
                    position = transformToCopy.position,
                    rotation = transformToCopy.rotation,
                    lossyScale = transformToCopy.lossyScale
                };
            }
        }

        [MenuItem("Edit/Paste Transform Values %#v", false, -101)]
        public static void PasteTransformValues() {
            foreach (var selection in Selection.gameObjects) {
                Transform targetTransform = selection.transform;
                Undo.RecordObject(targetTransform, "Paste Transform Values");

                targetTransform.position = copiedTransformValues.position;
                targetTransform.rotation = copiedTransformValues.rotation;
                targetTransform.scale(copiedTransformValues.lossyScale);
                if (targetTransform is RectTransform rt) {
                    rt.anchorMin = copiedTransformValues.anchorMin;
                    rt.anchorMax = copiedTransformValues.anchorMax;
                    rt.anchoredPosition = copiedTransformValues.anchoredPosition;
                    rt.pivot = copiedTransformValues.pivot;
                    rt.sizeDelta = copiedTransformValues.sizeDelta;
                }

            }
        }

        private struct TransformData {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 lossyScale;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 anchoredPosition;
            public Vector2 pivot;
            public Vector2 sizeDelta;
        }
        
    }

}