#if !UNITY_2021_2_OR_NEWER
using com.csutil.netstandard2_1polyfill;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui.viewstack {

    /// <summary> Can be attached next to a ViewStack to react to escape/back button clicks </summary>
    [RequireComponent(typeof(ViewStack))]
    class BackButtonListener : MonoBehaviour {

        public bool destroyFinalView = true;

        public void Update() {
            if (InputV2.GetKeyUp(KeyCode.Escape)) { // back button pressed
                Log.d("Back key pressed");
                var vs = gameObject.GetComponentV2<ViewStack>();
                var screenOnTop = GetCanvasWithHighestSortingOrder();
                var viewToClose = vs.GetLatestView();
                if (viewToClose.IsGrandChildOf(screenOnTop)) {
                    SwitchBackToLastViewViaBackKey(vs, viewToClose);
                } else {
                    SwitchBackToLastViewViaBackKey(vs, screenOnTop);
                }
            }
        }

        private static GameObject GetCanvasWithHighestSortingOrder() {
            var c = ResourcesV2.FindAllInScene<Canvas>();
            c = c.Filter(x => x.gameObject.activeInHierarchy && !x.HasComponent<IgnoreRootCanvas>(out var _));
            return c.OrderByDescending(x => x.sortingOrder).First().gameObject;
        }

        private void SwitchBackToLastViewViaBackKey(ViewStack viewStack, GameObject viewToClose) {
            Log.MethodEnteredWith("ViewStack" + viewStack, "viewToClose=" + viewToClose);
            if (!viewStack.SwitchBackToLastView(viewToClose)) {
                // The last view was reached so the switch back could not be performed
                if (destroyFinalView) { viewStack.DestroyViewStack(); }
            }
        }

    }

}