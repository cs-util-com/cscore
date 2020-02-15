using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace com.csutil.ui {

    public class CanvasFinder {

        public static Canvas GetOrAddRootCanvas() {
            var roots = GetAllRootCanvases();
            if (roots.IsNullOrEmpty()) {
                InitEventSystemIfNeeded();
                return ResourcesV2.LoadPrefab("Canvas/DefaultRootCanvas").GetComponent<Canvas>();
            }
            var firstCanvasOnSceneRootLevel = roots.FirstOrDefault(x => x.gameObject.GetParent() == null);
            if (firstCanvasOnSceneRootLevel != null) { return firstCanvasOnSceneRootLevel; }
            return roots.First();
        }

        /// <summary> Returns a list of root canvases where the first one is the visually most top canvas </summary>
        public static IOrderedEnumerable<Canvas> GetAllRootCanvases() {
            return ResourcesV2.FindAllInScene<Canvas>().Map(x => x.rootCanvas).ToHashSet().OrderByDescending(x => x.sortingOrder);
        }

        public static void InitEventSystemIfNeeded() {
            if (GameObject.FindObjectOfType<EventSystem>() == null) { ResourcesV2.LoadPrefab("Canvas/DefaultEventSystem"); }
        }
    }

}
