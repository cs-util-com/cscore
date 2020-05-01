using UnityEngine;

namespace com.csutil.ui {

    public class ViewStack : MonoBehaviour {

        public string screenToShowAsCloseView = "SideBar";
        protected GameObject activeCloseView;

        private void OnEnable() {
            RootCanvas.InitEventSystemIfNeeded();
        }

        /// <summary> Loads a new view based on its prefab name and by default hides the current one </summary>
        /// <param name="currentViewToHide"> (Any part of) the current view that should be hidden </param>
        /// <param name="prefabName"> e.g. "Dialogs/Dialog123" </param>
        /// <returns> The newly created view </returns>
        public GameObject ShowView(string prefabName, GameObject currentViewToHide = null, int siblingIndex = -1) {
            return ShowView(ResourcesV2.LoadPrefab(prefabName), currentViewToHide, siblingIndex);
        }

        /// <summary> Adds a passed view to the view stack to show it </summary>
        /// <param name="newView"> The new view to show in the stack </param>
        public GameObject ShowView(GameObject newView, GameObject currentViewToHide = null, int siblingIndex = -1) {
            gameObject.AddChild(newView, siblingIndex: siblingIndex);
            if (newView.GetComponentInParents<Canvas>() != null) { // The view is in a UI
                newView.GetOrAddComponent<RectTransform>().SetAnchorsStretchStretch();
            }
            EventBus.instance.Publish(EventConsts.catView + EventConsts.SHOW, newView);
            if (currentViewToHide != null) { GetRootViewOf(currentViewToHide).SetActiveV2(false); }
            return newView;
        }

        /// <summary> Returns the latest view on the ViewStack that is active. Or null if none is found </summary>
        public GameObject GetLatestView() {
            var viewCount = gameObject.GetChildCount();
            for (int i = viewCount - 1; i >= 0; i--) {
                var child = gameObject.GetChild(i);
                if (child.activeSelf) { return child; }
            }
            return null;
        }

        /// <summary> Destroys the complete ViewStack including all views </summary>
        public void DestroyViewStack() { gameObject.Destroy(); }

        /// <summary> Will "close" the current view and jump back to the last view and set it back to active </summary>
        /// <param name="gameObject"> The current view or any part of it </param>
        /// <param name="destroyFinalView"> If true and the last view on the stack is reached this last view will be destroyed too </param>
        /// <param name="hideNotDestroyCurrentView"> If set to true the current active view will not be destroyed but instead set to hidden </param>
        /// <returns></returns>
        public bool SwitchBackToLastView(GameObject gameObject, bool destroyFinalView = false, bool hideNotDestroyCurrentView = false) {
            var currentView = GetRootViewOf(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            if (currentIndex > 0) {
                var lastView = transform.GetChild(currentIndex - 1).gameObject;
                lastView.SetActiveV2(true);
                EventBus.instance.Publish(EventConsts.catView + EventConsts.SWITCH_BACK_TO_LAST, "" + currentView, lastView);
            } else {
                if (!screenToShowAsCloseView.IsNullOrEmpty() && activeCloseView == null) {
                    try {
                        activeCloseView = ShowView(screenToShowAsCloseView, siblingIndex: 0);
                        return true;
                    }
                    catch (System.Exception e) { Log.w("Could not show screenToShowAsCloseView=" + screenToShowAsCloseView, e); }
                }
                if (!destroyFinalView) { return false; }
            }
            if (hideNotDestroyCurrentView) {
                return currentView.SetActiveV2(false);
            } else {
                return currentView.Destroy();
            }
        }

        /// <summary> Will show the next view on the view stack and by default automatically hide the current view </summary>
        /// <param name="gameObject"> The current view or any part of it </param>
        /// <param name="hideCurrentView"> If false the current view will stay visible, relevant e.g. for transparent new views </param>
        /// <returns> True if there was a next view to show, false otherwise </returns>
        public bool SwitchToNextView(GameObject gameObject, bool hideCurrentView = true) {
            var currentView = GetRootViewOf(gameObject);
            var currentIndex = currentView.transform.GetSiblingIndex();
            if (currentIndex == transform.childCount - 1) { Log.w("Current was last view in the stack"); }
            if (currentIndex >= transform.childCount - 1) { return false; }
            var nextView = transform.GetChild(currentIndex + 1).gameObject;
            nextView.SetActiveV2(true);
            EventBus.instance.Publish(EventConsts.catView + EventConsts.SWITCH_TO_NEXT, currentView, nextView);
            if (hideCurrentView) { currentView.SetActiveV2(false); }
            return true;
        }

        /// <summary> Moves up the tree until it reaches the direct child (the view) of the viewstack </summary>
        public GameObject GetRootViewOf(GameObject viewElement) {
            if (viewElement == gameObject) { throw Log.e("Cant get root for ViewStack gameobject"); }
            var parent = viewElement.GetParent();
            if (parent == gameObject) { return viewElement; } // stop when the GO of the viewstack is reached
            return GetRootViewOf(parent);
        }

    }

}