using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public static class RootCanvas {

        public static Canvas GetOrAddRootCanvas() {
            var roots = GetAllRootCanvases();
            if (roots.IsNullOrEmpty()) { return CreateNewRootCanvas(); }
            // Check if there is a root canvas that has a ViewStack attached:
            var rootCanvasesWithViewStack = roots.Filter(x => x.GetComponent<ViewStack>() != null);
            if (!rootCanvasesWithViewStack.IsNullOrEmpty()) {
                AssertV2.AreEqual(1, rootCanvasesWithViewStack.Count(), "rootCanvasesWithViewStack");
                return rootCanvasesWithViewStack.First();
            }
            // Prefer canvas objects that are on the root level of the open scene:
            var canvasesOnRootOfScene = roots.Filter(x => x.gameObject.GetParent() == null);
            if (canvasesOnRootOfScene.IsNullOrEmpty()) {
                AssertV2.AreEqual(1, canvasesOnRootOfScene.Count(), "canvasesOnRootOfScene");
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
    }

}
