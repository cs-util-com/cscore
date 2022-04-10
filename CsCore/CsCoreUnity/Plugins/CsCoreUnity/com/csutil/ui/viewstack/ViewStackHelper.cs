using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui {

    public static class ViewStackHelper {

        public static ViewStack GetOrAddMainViewStack() { return GetOrAdd("MainViewStack"); }

        public static ViewStack GetOrAdd(string viewStackName) {
            var c = RootCanvas.GetOrAddRootCanvasV2();

            var viewStacks = c.gameObject.GetChildren();
            AssertAllAreViewStacks(viewStacks);

            var go = viewStacks.SingleOrDefault(x => x.name == viewStackName);
            if (go == null) {
                return c.gameObject.AddChild(new GameObject(viewStackName)).AddComponent<ViewStack>();
            }
            if (go.GetComponentV2<ViewStack>() == null) {
                throw Log.e("GO found with correct name but it did not have any viewstack attached: " + go, go);
            }
            return go.GetComponentV2<ViewStack>();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private static void AssertAllAreViewStacks(List<GameObject> viewStacks) {
            foreach (var go in viewStacks) {
                if (go.GetComponentV2<ViewStack>() == null) {
                    throw Log.e("Missing ViewStack in direct child of root canvas: " + go, go);
                }
            }
        }

    }

}