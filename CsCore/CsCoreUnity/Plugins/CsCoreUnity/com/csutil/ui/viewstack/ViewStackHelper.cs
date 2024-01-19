using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui {

    public static class ViewStackHelper {

        public static ViewStack MainViewStack() {
            return RootCanvas.GetOrAddRootCanvasV2().GetOrAddViewStack("Canvas/MainViewStack");
        }

        public static ViewStack GetOrAddViewStack(this Canvas self, string viewStackPrefabName) {
            AssertV2.IsTrue(self.isRootCanvasV2(), "Passed canvas was not a root canvas", self.gameObject);
            var canvasGO = self.gameObject;
            var viewStackGOs = canvasGO.GetChildren();
            var go = viewStackGOs.SingleOrDefault(x => x.name == viewStackPrefabName);
            var viewstack = go != null ? go.GetComponentV2<ViewStack>() : canvasGO.AddChild(ResourcesV2.LoadPrefab(viewStackPrefabName)).GetComponentV2<ViewStack>();
            if (viewstack == null) { throw Log.e("No ViewStack found in GameObject " + go, go); }
            return viewstack;
        }

        public static GameObject SwitchToView(this ViewStack target, string prefabName, int siblingIndex = -1) {
            return target.ShowView(prefabName, target.GetLatestView(), siblingIndex);
        }
        
        public static GameObject ShowView(this ViewStack target, string prefabName, int siblingIndex = -1) {
            return target.ShowView(prefabName, null, siblingIndex);
        }

        public static GameObject SwitchToView(this ViewStack target, GameObject newView, int siblingIndex = -1) {
            return target.ShowView(newView, target.GetLatestView(), siblingIndex);
        }

    }

}