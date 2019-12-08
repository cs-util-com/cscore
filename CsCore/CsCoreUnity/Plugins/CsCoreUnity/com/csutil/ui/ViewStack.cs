using UnityEngine;

namespace com.csutil.ui {

    public class ViewStack : MonoBehaviour {

        public GameObject ShowView(GameObject gameObject, string prefabName, bool hideCurrentView = true) {
            return ShowView(gameObject, ResourcesV2.LoadPrefab(prefabName), hideCurrentView);
        }

        /// <summary> Adds a passed view to the view stack to show it, by default hides the current view on the stack</summary>
        /// <param name="newView"> The new view to show in the stack </param>
        /// <param name="hideCurrentView"> If false the current view will stay visible, relevant e.g. for transparent new views </param>
        /// <returns> The new view that was just added to the stack, useful for chaining </returns>
        public GameObject ShowView(GameObject gameObject, GameObject newView, bool hideCurrentView = true) {
            var view = AddView(newView);
            EventBus.instance.Publish(UiEvents.SHOW_VIEW, view);
            if (hideCurrentView) { GetRootFor(gameObject).SetActive(false); }
            return view;
        }

        private GameObject AddView(GameObject newView) { return gameObject.AddChild(newView); }

        /// <summary> Will "close" the current view and jump back to the last view and set it back to active </summary>
        /// <param name="destroyFinalView"> If true and the last view on the stack is reached this last view will be destroyed too </param>
        /// <param name="hideNotDestroyCurrentView"> If set to true the current active view will not be destroyed but instead set to hidden </param>
        public bool SwitchBackToLastView(GameObject gameObject, bool destroyFinalView = false, bool hideNotDestroyCurrentView = false) {
            var currentView = GetRootFor(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            AssertV2.AreEqual(currentIndex, transform.childCount - 1, "Current was not last view in the stack");
            if (currentIndex > 0) {
                var lastView = transform.GetChild(currentIndex - 1).gameObject;
                lastView.SetActive(true);
                EventBus.instance.Publish(UiEvents.SWITCH_BACK_TO_LAST_VIEW, "" + currentView, lastView);
            }
            if (currentIndex == 0 && !destroyFinalView) { return false; }
            if (hideNotDestroyCurrentView) {
                return currentView.SetActiveV2(false);
            } else {
                return currentView.Destroy();
            }
        }

        /// <summary> Will show the next view on the view stack and by default automatically hide the current view </summary>
        /// <param name="hideCurrentView"> If false the current view will stay visible, relevant e.g. for transparent new views </param>
        /// <returns> True if there was a next view to show, false otherwise </returns>
        public bool SwitchToNextView(GameObject gameObject, bool hideCurrentView = true) {
            var currentView = GetRootFor(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            if (currentIndex == transform.childCount - 1) { Log.w("Current was last view in the stack"); }
            if (currentIndex >= transform.childCount - 1) { return false; }
            var nextView = transform.GetChild(currentIndex + 1).gameObject;
            nextView.SetActive(true);
            EventBus.instance.Publish(UiEvents.SWITCH_TO_NEXT_VIEW, currentView, nextView);
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