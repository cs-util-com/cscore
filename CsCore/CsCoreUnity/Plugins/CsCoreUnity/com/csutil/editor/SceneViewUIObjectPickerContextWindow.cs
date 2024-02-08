using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2021_2_OR_NEWER
 using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
 using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#elif UNITY_2018_3_OR_NEWER
 using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
 using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace com.csutil.editor {

    /// <summary>
    /// "Right click anywhere in Scene view to show a context menu displaying the UI objects under the cursor"
    /// From https://gist.github.com/yasirkula/06edc780beaa4d8705b3564d60886fa6
    /// </summary>
    public class SceneViewUIObjectPickerContextWindow : EditorWindow {
        private struct Entry {
            public readonly RectTransform RectTransform;
            public readonly List<Entry> Children;

            public Entry(RectTransform rectTransform) {
                RectTransform = rectTransform;
                Children = new List<Entry>(2);
            }
        }

        private readonly List<RectTransform> uiObjects = new List<RectTransform>(16);
        private readonly List<string> uiObjectLabels = new List<string>(16);

        private static RectTransform hoveredUIObject;
        private static readonly Vector3[] hoveredUIObjectCorners = new Vector3[4];
        private static readonly List<ICanvasRaycastFilter> raycastFilters = new List<ICanvasRaycastFilter>(4);

        private static double lastRightClickTime;
        private static Vector2 lastRightPos;
        private static bool blockSceneViewInput;

        private readonly MethodInfo screenFittedRectGetter = typeof(EditorWindow).Assembly.GetType("UnityEditor.ContainerWindow").GetMethod("FitRectToScreen", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

        private const float Padding = 1f;
        private float RowHeight { get { return EditorGUIUtility.singleLineHeight; } }
        private GUIStyle RowGUIStyle { get { return "MenuItem"; } }

        private void ShowContextWindow(List<Entry> results) {
            StringBuilder sb = new StringBuilder(100);
            InitializeUIObjectsRecursive(results, 0, sb);

            GUIStyle rowGUIStyle = RowGUIStyle;
            float preferredWidth = 0f;
            foreach (string label in uiObjectLabels)
                preferredWidth = Mathf.Max(preferredWidth, rowGUIStyle.CalcSize(new GUIContent(label)).x);

            ShowAsDropDown(new Rect(), new Vector2(preferredWidth + Padding * 2f, uiObjects.Count * RowHeight + Padding * 2f));

            // Show dropdown above the cursor instead of below the cursor
            position = (Rect)screenFittedRectGetter.Invoke(null, new object[3] { new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition) - new Vector2(0f, position.height), position.size), true, true });
        }

        private void InitializeUIObjectsRecursive(List<Entry> results, int depth, StringBuilder sb) {
            foreach (Entry entry in results) {
                sb.Length = 0;

                uiObjects.Add(entry.RectTransform);
                uiObjectLabels.Add(sb.Append(' ', depth * 4).Append(entry.RectTransform.name).ToString());

                if (entry.Children.Count > 0)
                    InitializeUIObjectsRecursive(entry.Children, depth + 1, sb);
            }
        }

        protected void OnEnable() {
            wantsMouseMove = wantsMouseEnterLeaveWindow = true;
#if UNITY_2020_1_OR_NEWER
            wantsLessLayoutEvents = false;
#endif
            blockSceneViewInput = true;
        }

        protected void OnDisable() {
            hoveredUIObject = null;
            SceneView.RepaintAll();
        }

        protected void OnGUI() {
            Event ev = Event.current;

            float rowWidth = position.width - Padding * 2f, rowHeight = RowHeight;
            GUIStyle rowGUIStyle = RowGUIStyle;
            int hoveredRowIndex = -1;
            for (int i = 0; i < uiObjects.Count; i++) {
                Rect rect = new Rect(Padding, Padding + i * rowHeight, rowWidth, rowHeight);
                if (GUI.Button(rect, uiObjectLabels[i], rowGUIStyle)) {
                    if (uiObjects[i] != null)
                        Selection.activeTransform = uiObjects[i];

                    blockSceneViewInput = false;
                    ev.Use();
                    Close();
                    GUIUtility.ExitGUI();
                }

                if (hoveredRowIndex < 0 && ev.type == EventType.MouseMove && rect.Contains(ev.mousePosition))
                    hoveredRowIndex = i;
            }

            if (ev.type == EventType.MouseMove || ev.type == EventType.MouseLeaveWindow) {
                RectTransform hoveredUIObject = (hoveredRowIndex >= 0) ? uiObjects[hoveredRowIndex] : null;
                if (hoveredUIObject != SceneViewUIObjectPickerContextWindow.hoveredUIObject) {
                    SceneViewUIObjectPickerContextWindow.hoveredUIObject = hoveredUIObject;
                    Repaint();
                    SceneView.RepaintAll();
                }
            }
        }

        [InitializeOnLoadMethod]
        private static void OnSceneViewGUI() {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += (SceneView sceneView) =>
#else
		SceneView.onSceneGUIDelegate += ( SceneView sceneView ) =>
#endif
            {
                /// Couldn't get <see cref="EventType.ContextClick"/> to work here in Unity 5.6 so implemented context click detection manually
                Event ev = Event.current;
                switch (ev.type) {
                    case EventType.MouseDown: {
                        if (ev.button == 1) {
                            lastRightClickTime = EditorApplication.timeSinceStartup;
                            lastRightPos = ev.mousePosition;
                        } else if (blockSceneViewInput) {
                            // User has clicked outside the context window to close it. Ignore this click in Scene view if it's left click
                            blockSceneViewInput = false;

                            if (ev.button == 0) {
                                GUIUtility.hotControl = 0;
                                ev.Use();
                            }
                        }

                        break;
                    }
                    case EventType.MouseUp: {
                        if (ev.button == 1 && EditorApplication.timeSinceStartup - lastRightClickTime < 0.2 && (ev.mousePosition - lastRightPos).magnitude < 2f)
                            OnSceneViewRightClicked(sceneView);

                        break;
                    }
                }

                if (hoveredUIObject != null) {
                    hoveredUIObject.GetWorldCorners(hoveredUIObjectCorners);
                    Handles.DrawSolidRectangleWithOutline(hoveredUIObjectCorners, new Color(1f, 1f, 0f, 0.25f), Color.black);
                }
            };
        }

        private static void OnSceneViewRightClicked(SceneView sceneView) {
            // Find all UI objects under the cursor
            Vector2 pointerPos = HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition);
            Entry rootEntry = new Entry(null);
#if UNITY_2018_3_OR_NEWER
            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null && prefabStage.stageHandle.IsValid() && prefabStage.prefabContentsRoot.transform is RectTransform prefabStageRoot)
                CheckRectTransformRecursive(prefabStageRoot, pointerPos, sceneView.camera, false, rootEntry.Children);
            else
#endif
            {
#if UNITY_2022_3_OR_NEWER
                Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
#else
			Canvas[] canvases = FindObjectsOfType<Canvas>();
#endif
                Array.Sort(canvases, (c1, c2) => c1.sortingOrder.CompareTo(c2.sortingOrder));
                foreach (Canvas canvas in canvases) {
                    if (canvas != null && canvas.gameObject.activeInHierarchy && canvas.isRootCanvas)
                        CheckRectTransformRecursive((RectTransform)canvas.transform, pointerPos, sceneView.camera, false, rootEntry.Children);
                }
            }

            // Remove non-Graphic root entries with no children from the results
            rootEntry.Children.RemoveAll((canvasEntry) => canvasEntry.Children.Count == 0 && !canvasEntry.RectTransform.GetComponent<Graphic>());

            // If any results found, show the context window
            if (rootEntry.Children.Count > 0)
                CreateInstance<SceneViewUIObjectPickerContextWindow>().ShowContextWindow(rootEntry.Children);
        }

        private static void CheckRectTransformRecursive(RectTransform rectTransform, Vector2 pointerPos, Camera camera, bool culledByCanvasGroup, List<Entry> result) {
            Canvas canvas = rectTransform.GetComponent<Canvas>();
            if (canvas != null && !canvas.enabled)
                return;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, pointerPos, camera) && ShouldCheckRectTransform(rectTransform, pointerPos, camera, ref culledByCanvasGroup)) {
                Entry entry = new Entry(rectTransform);
                result.Add(entry);
                result = entry.Children;
            }

            for (int i = 0, childCount = rectTransform.childCount; i < childCount; i++) {
                RectTransform childRectTransform = rectTransform.GetChild(i) as RectTransform;
                if (childRectTransform != null && childRectTransform.gameObject.activeSelf)
                    CheckRectTransformRecursive(childRectTransform, pointerPos, camera, culledByCanvasGroup, result);
            }
        }

        private static bool ShouldCheckRectTransform(RectTransform rectTransform, Vector2 pointerPos, Camera camera, ref bool culledByCanvasGroup) {
#if UNITY_2019_3_OR_NEWER
            if (SceneVisibilityManager.instance.IsHidden(rectTransform.gameObject, false))
                return false;

            if (SceneVisibilityManager.instance.IsPickingDisabled(rectTransform.gameObject, false))
                return false;
#endif

            CanvasRenderer canvasRenderer = rectTransform.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null && canvasRenderer.cull)
                return false;

            CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if (canvasGroup != null) {
                if (canvasGroup.ignoreParentGroups)
                    culledByCanvasGroup = canvasGroup.alpha == 0f;
                else if (canvasGroup.alpha == 0f)
                    culledByCanvasGroup = true;
            }

            if (!culledByCanvasGroup) {
                // If the target is a MaskableGraphic that ignores masks (i.e. visible outside masks) and isn't fully transparent, accept it
                MaskableGraphic maskableGraphic = rectTransform.GetComponent<MaskableGraphic>();
                if (maskableGraphic != null && !maskableGraphic.maskable && maskableGraphic.color.a > 0f)
                    return true;

                raycastFilters.Clear();
                rectTransform.GetComponentsInParent(false, raycastFilters);
                foreach (var raycastFilter in raycastFilters) {
                    if (!raycastFilter.IsRaycastLocationValid(pointerPos, camera))
                        return false;
                }
            }

            return !culledByCanvasGroup;
        }
    }

}