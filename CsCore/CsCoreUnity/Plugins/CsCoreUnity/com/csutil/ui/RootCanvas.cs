#if !UNITY_2021_2_OR_NEWER
using com.csutil.netstandard2_1polyfill;
#endif
using System;
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
            var rootCanvasesWithViewStack = roots.Filter(x => x.GetComponent<ViewStack>() != null);
            if (!rootCanvasesWithViewStack.IsNullOrEmpty()) {
                AssertV2.AreEqual(1, rootCanvasesWithViewStack.Count(), "rootCanvasesWithViewStack");
                return rootCanvasesWithViewStack.First();
            }
            return FilterForBestRootCanvas(roots);
        }

        private static Canvas FilterForBestRootCanvas(IOrderedEnumerable<Canvas> roots) {
            // Prefer canvas objects that are on the root level of the open scene:
            var canvasesOnRootOfScene = roots.Filter(x => x.gameObject.GetParent() == null);
            if (!canvasesOnRootOfScene.IsNullOrEmpty()) {
                int nr = canvasesOnRootOfScene.Count();
                if (nr != 1) { Log.w($"Found {nr} root-canvases on the top level of the scene. Will use the first found one.."); }
                return canvasesOnRootOfScene.First();
            }
            // As a fallback return the first root canvas:
            return roots.First();
        }

        /// <summary> Returns a list of root canvases where the first one is the visually most top canvas </summary>
        public static IOrderedEnumerable<Canvas> GetAllRootCanvases() {
            return ResourcesV2.FindAllInScene<Canvas>().Map(x => x.rootCanvas).ToHashSet().OrderByDescending(x => x.sortingOrder);
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
            var rootCanvasesWithViewStack = roots.Filter(x => x.GetComponent<ViewStack>() != null);
            if (!rootCanvasesWithViewStack.IsNullOrEmpty()) {
                foreach (var c in rootCanvasesWithViewStack) {
                    throw Log.e("Found root canvas which had a ViewStack directly attached to it, consider moving the ViewStack to a direct child of the root canvas instead", c.gameObject);
                }
            }
        }

    }

}