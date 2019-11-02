using UnityEngine;

namespace com.csutil.ui {

    public class ViewStack : MonoBehaviour {

        public GameObject ShowView(GameObject gameObject, string prefabName, bool hideCurrentView = true) {
            return ShowView(gameObject, ResourcesV2.LoadPrefab(prefabName), hideCurrentView);
        }

        public GameObject ShowView(GameObject gameObject, GameObject newView, bool hideCurrentView = true) {
            var v = AddView(newView);
            EventBus.instance.Publish(UiEvents.SHOW_VIEW, v);
            if (hideCurrentView) { GetRootFor(gameObject).SetActive(false); }
            return v;
        }

        private GameObject AddView(GameObject newView) { return gameObject.AddChild(newView); }

        public bool SwitchBackToLastView(GameObject gameObject, bool destroyFinalView = false) {
            var currentView = GetRootFor(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            AssertV2.AreEqual(currentIndex, transform.childCount - 1, "Current was not last view in the stack");
            if (currentIndex > 0) {
                var lastView = transform.GetChild(currentIndex - 1).gameObject;
                lastView.SetActive(true);
                EventBus.instance.Publish(UiEvents.SWITCH_BACK_TO_LAST_VIEW, lastView);
            }
            if (currentIndex == 0 && !destroyFinalView) { return false; }
            return currentView.Destroy();
        }

        public bool SwitchToNextView(GameObject gameObject, bool hideCurrentView = true) {
            var currentView = GetRootFor(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            if (currentIndex == transform.childCount - 1) { Log.w("Current was last view in the stack"); }
            if (currentIndex < transform.childCount - 1) {
                var nextView = transform.GetChild(currentIndex + 1).gameObject;
                nextView.SetActive(true);
                EventBus.instance.Publish(UiEvents.SWITCH_TO_NEXT_VIEW, nextView);
            }
            if (hideCurrentView) { currentView.SetActive(false); }
            return true;
        }

        /// <summary> Moves up the tree until it reaches the direct child of the viewstack </summary>
        private GameObject GetRootFor(GameObject go) {
            AssertV2.IsFalse(go == gameObject, "Cant get root for ViewStack gameobject");
            var parent = go.GetParent();
            if (parent == gameObject) { return go; } // stop when the GO of the viewstack is reached
            return GetRootFor(parent);
        }

    }

}