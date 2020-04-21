using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui.viewstack {

    /// <summary> Can be attached next to a ViewStack to react to escape/back button clicks </summary>
    [RequireComponent(typeof(ViewStack))]
    class BackButtonListener : MonoBehaviour {

        public bool destroyFinalView = true;

        public void Update() {
            if (Input.GetKeyUp(KeyCode.Escape)) { // back button pressed
                Log.d("Back key pressed");
                var vs = gameObject.GetComponent<ViewStack>();
                var c = vs.gameObject.GetComponentInParents<Canvas>()?.rootCanvas;
                if (c != null && c == RootCanvas.GetAllRootCanvases().First()) {
                    var sortedViews = SortByCanvasSortingOrder(vs.gameObject.GetChildrenIEnumerable());
                    if (!vs.SwitchBackToLastView(sortedViews.First())) {
                        // The last view was reached so the switch back could not be performed
                        if (destroyFinalView) { vs.DestroyViewStack(); }
                    }
                }
            }
        }

        private static IOrderedEnumerable<GameObject> SortByCanvasSortingOrder(IEnumerable<GameObject> c) {
            return c.ToHashSet().OrderByDescending(x => x.GetComponent<Canvas>().sortingOrder);
        }

    }

}