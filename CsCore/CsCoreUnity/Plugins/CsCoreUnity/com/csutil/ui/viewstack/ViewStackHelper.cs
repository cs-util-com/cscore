using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui {

    public static class ViewStackHelper {

        public static ViewStack GetOrAddMainViewStack() { return GetOrAdd("Canvas/MainViewStack"); }

        public static ViewStack GetOrAdd(string viewStackPrefabName) {
            var rootCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject;
            var viewStackGOs = rootCanvas.GetChildren();
            var go = viewStackGOs.SingleOrDefault(x => x.name == viewStackPrefabName);
            var viewstack = go != null ? go.GetComponentV2<ViewStack>() : rootCanvas.AddChild(ResourcesV2.LoadPrefab(viewStackPrefabName)).GetComponentV2<ViewStack>();
            if (viewstack == null) { throw Log.e("No ViewStack found in GameObject " + go, go); }
            return viewstack;
        }

    }

}