using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.ui {

    public class ViewStack : MonoBehaviour {

        public static ViewStack GetViewStack(GameObject gameObject) {
            return gameObject.GetComponentInParent<ViewStack>();
        }

        public static GameObject SwitchToScreen(GameObject gameObject, string prefabName, bool hideCurrentScreen = true) {
            return SwitchToScreen(gameObject, ResourcesV2.LoadPrefab(prefabName), hideCurrentScreen);
        }

        public static GameObject SwitchToScreen(GameObject gameObject, GameObject newScreen, bool hideCurrentScreen = true) {
            var stack = GetViewStack(gameObject);
            var op = stack.AddScreen(newScreen);
            if (hideCurrentScreen) { stack.GetRootFor(gameObject).SetActive(false); }
            return op;
        }

        public static bool SwitchBackToLastScreen(GameObject gameObject, bool destroyFinalScreen = false) {
            var viewStack = GetViewStack(gameObject);
            if (viewStack == null) { return false; }
            var currentScreen = viewStack.GetRootFor(gameObject);
            var currentIndex = currentScreen.transform.GetSiblingIndex();
            AssertV2.AreEqual(currentIndex, viewStack.transform.childCount - 1, "Current was not last screen in the stack");
            if (currentIndex > 0) {
                var lastScreen = viewStack.transform.GetChild(currentIndex - 1).gameObject;
                lastScreen.SetActive(true);
            }
            if (destroyFinalScreen || currentIndex > 0) { currentScreen.Destroy(); }
            return true;
        }

        public static bool SwitchToNextScreen(GameObject gameObject, bool hideCurrentScreen = true) {
            var viewStack = GetViewStack(gameObject);
            if (viewStack == null) { return false; }
            var currentScreen = viewStack.GetRootFor(gameObject);
            var currentIndex = currentScreen.transform.GetSiblingIndex();
            AssertV2.AreNotEqual(currentIndex, viewStack.transform.childCount - 1, "Current was last screen in the stack");
            if (currentIndex < viewStack.transform.childCount - 1) {
                var lastScreen = viewStack.transform.GetChild(currentIndex - 1).gameObject;
                lastScreen.SetActive(true);
            }
            if (hideCurrentScreen) { currentScreen.SetActive(false); }
            return true;
        }

        /// <summary> Moves up the tree until it reaches the direct child of the viewstack </summary>
        private GameObject GetRootFor(GameObject go) {
            AssertV2.IsFalse(go == gameObject, "Cant get root for ViewStack gameobject");
            var parent = go.GetParent();
            if (parent == gameObject) { return go; }
            return GetRootFor(parent);
        }

        public GameObject AddScreen(GameObject newScreen) { return gameObject.AddChild(newScreen); }

    }

}
