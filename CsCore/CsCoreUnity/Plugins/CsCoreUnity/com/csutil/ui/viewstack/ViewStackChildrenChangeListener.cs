using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui {

    [RequireComponent(typeof(ViewStack))] // Make sure this has to be placed on the level of the viewstack
    public class ViewStackChildrenChangeListener : MonoBehaviour {

        private List<GameObject> oldDirectChildren;

        private void OnTransformChildrenChanged() {
            var directChildren = gameObject.GetChildren();
            if (oldDirectChildren != null) {
                foreach (var child in oldDirectChildren.Except(directChildren)) { OnViewRemoved(child); }
                foreach (var child in directChildren.Except(oldDirectChildren)) { OnViewAdded(child); }
            }
            oldDirectChildren = directChildren;
        }

        protected virtual void OnViewAdded(GameObject addedView) {
            EventBus.instance.Publish(EventConsts.catView + EventConsts.ADDED, addedView);
        }

        protected virtual void OnViewRemoved(GameObject removedView) {
            EventBus.instance.Publish(EventConsts.catView + EventConsts.REMOVED, removedView);
        }

    }

}