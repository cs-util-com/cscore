#if !UNITY_2021_2_OR_NEWER
using com.csutil.netstandard2_1polyfill;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public static class RootCanvas {

        /// <summary> Returns a root canvas that will contain the different view stacks like the main view stack, the viewstack for toast, the viewstack for progress overlays, .. </summary>
        public static Canvas GetOrAddRootCanvasV2() {
            var roots = GetAllRootCanvases();
            if (roots.IsNullOrEmpty()) { return CreateNewRootCanvas("Canvas/DefaultRootCanvasV2"); }
            AssertNoViewStacksOnRootCanvasLevel(roots);
            return FilterForBestRootCanvas(roots);
        }

        [Obsolete("Recommended to use version 2 instead + not using a single viewstack for any screens")]
        public static Canvas GetOrAddRootCanvas() {
            var roots = GetAllRootCanvases();
            if (roots.IsNullOrEmpty()) { return CreateNewRootCanvas(); }
            // Check if there is a root canvas that has a ViewStack attached:
            var rootCanvasesWithViewStack = roots.Filter(x => x.GetComponentV2<ViewStack>() != null);
            if (!rootCanvasesWithViewStack.IsNullOrEmpty()) {
                AssertV2.AreEqual(1, rootCanvasesWithViewStack.Count(), "rootCanvasesWithViewStack");
                return rootCanvasesWithViewStack.First();
            }
            return FilterForBestRootCanvas(roots);
        }

        private static Canvas FilterForBestRootCanvas(IEnumerable<Canvas> roots) {

            // If some of the canvases are in screen space overlay mode, prefer those:
            var screenSpaceCanvases = roots.Filter(x => x.renderMode == RenderMode.ScreenSpaceOverlay);
            if (!screenSpaceCanvases.IsEmpty()) { roots = screenSpaceCanvases; }

            // Prefer canvas objects that are on the root level of the open scene:
            var canvasesOnRootOfScene = roots.Filter(x => x.gameObject.GetParent() == null);
            if (!canvasesOnRootOfScene.IsNullOrEmpty()) {
                int nr = canvasesOnRootOfScene.Count();
                if (nr != 1) {
                    var canvasesWithViewStacks = canvasesOnRootOfScene.Filter(c => c.GetComponentInChildren<ViewStack>());
                    if (!canvasesWithViewStacks.IsNullOrEmpty()) { canvasesOnRootOfScene = canvasesWithViewStacks; }
                    var firstCanvasInList = canvasesOnRootOfScene.First();
                    Log.w($"Found {nr} root-canvases on the top level of the scene. Will use the first found Canvas ({firstCanvasInList})", firstCanvasInList.gameObject);
                }
                return canvasesOnRootOfScene.First();
            }
            // As a fallback return the first root canvas:
            return roots.First();
        }

        /// <summary> Returns a list of root canvases where the first one is the visually most top canvas </summary>
        public static IOrderedEnumerable<Canvas> GetAllRootCanvases() {
            return ResourcesV2.FindAllInScene<Canvas>().Map(x => x.rootCanvasV2()).ToHashSet().Filter(x => !(x.HasComponent<IgnoreRootCanvas>(out var i) && i.enabled)).OrderByDescending(x => x.sortingOrder);
        }

        public static Canvas CreateNewRootCanvas(string rootCanvasPrefab = "Canvas/DefaultRootCanvas") {
            InitEventSystemIfNeeded();
            return ResourcesV2.LoadPrefab(rootCanvasPrefab).GetComponentV2<Canvas>();
        }

        public static void InitEventSystemIfNeeded() {
            if (GameObject.FindObjectOfType<EventSystem>() == null) { ResourcesV2.LoadPrefab("Canvas/DefaultEventSystem"); }
        }

        /// <summary> Assert that none of the root canvases has a viewstack directly attached to the same level </summary>
        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private static void AssertNoViewStacksOnRootCanvasLevel(IOrderedEnumerable<Canvas> roots) {
            var rootCanvasesWithViewStack = roots.Filter(x => x.GetComponentV2<ViewStack>() != null);
            if (!rootCanvasesWithViewStack.IsNullOrEmpty()) {
                foreach (var c in rootCanvasesWithViewStack) {
                    Log.e("Found root canvas which had a ViewStack directly attached to it, consider moving the ViewStack to a direct child of the root canvas instead", c.gameObject);
                }
            }
        }

        public static Canvas rootCanvasV2(this Canvas self) {
            var rootCanvas = self.rootCanvas;
            if (self == rootCanvas) {
                if (!rootCanvas.isRootCanvasV2()) {
                    var realRootCanvas = SearchForParentCanvas(rootCanvas);
                    return realRootCanvas;
                }
            }
            return rootCanvas;
        }

        public static bool isRootCanvasV2(this Canvas self) {
            if (self == null) { return false; }
            if (!self.isRootCanvas) return false;

            // There is a bug that during the onEnable phase of a MonoBehavior a canvas thinks it is a
            // root canvas even though it is not, so additionally all parent canvases need to be collected up
            // to the root of the GameObject tree to ensure there are no other canvases on the way up.

            // If a canvas is found in any grandparent the current canvas who thinks its a root canvas cant be one:
            var parentCanvas = SearchForParentCanvas(self);
            if (parentCanvas != null) {
                LogWarningNotToDoUiOperationsDuringOnEnable(self);
                return false;
            }
            return true;
        }

        private static Canvas SearchForParentCanvas(Canvas self) {
            var parent = self.gameObject.GetParent();
            return parent?.GetComponentInParents<Canvas>();
        }

        [Conditional("DEBUG")]
        private static void LogWarningNotToDoUiOperationsDuringOnEnable(Canvas self) {
            // Log.w("Using operations on canvas such as .isRootCanvas during onEnable can result in incorrect UI results! If possible delay such operations until the UI is initialized", self.gameObject);
        }

    }

}