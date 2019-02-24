using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.ui {

    public class ScreenStack : MonoBehaviour {

        public static ScreenStack GetScreenStack(GameObject gameObject) {
            return gameObject.GetComponentInParent<ScreenStack>();
        }

        public static GameObject SwitchToScreen(GameObject gameObject, string prefabName, bool hideCurrentScreen = true) {
            return SwitchToScreen(gameObject, ResourcesV2.LoadPrefab(prefabName), hideCurrentScreen);
        }

        public static GameObject SwitchToScreen(GameObject gameObject, GameObject newScreen, bool hideCurrentScreen = true) {
            var stack = GetScreenStack(gameObject);
            var op = stack.AddScreen(newScreen);
            if (hideCurrentScreen) { stack.GetRootFor(gameObject).SetActive(false); }
            return op;
        }

        public static bool SwitchBackToLastScreen(GameObject gameObject) {
            var screenStack = GetScreenStack(gameObject);
            if (screenStack == null) { return false; }
            var currentScreen = screenStack.GetRootFor(gameObject);
            var currentIndex = currentScreen.transform.GetSiblingIndex();
            AssertV2.AreEqual(currentIndex, screenStack.transform.childCount - 1, "Current was not last screen in the stack");
            if (currentIndex > 0) {
                var lastScreen = screenStack.transform.GetChild(currentIndex - 1).gameObject;
                lastScreen.SetActive(true);
            }
            currentScreen.Destroy();
            return true;
        }

        public static bool SwitchToNextScreen(GameObject gameObject, bool hideCurrentScreen = true) {
            var screenStack = GetScreenStack(gameObject);
            if (screenStack == null) { return false; }
            var currentScreen = screenStack.GetRootFor(gameObject);
            var currentIndex = currentScreen.transform.GetSiblingIndex();
            AssertV2.AreNotEqual(currentIndex, screenStack.transform.childCount - 1, "Current was last screen in the stack");
            if (currentIndex < screenStack.transform.childCount - 1) {
                var lastScreen = screenStack.transform.GetChild(currentIndex - 1).gameObject;
                lastScreen.SetActive(true);
            }
            if (hideCurrentScreen) { currentScreen.SetActive(false); }
            return true;
        }

        /// <summary> Moves up the tree until it reaches the direct child of the screenstack </summary>
        private GameObject GetRootFor(GameObject go) {
            AssertV2.IsFalse(go == gameObject, "Cant get root for ScreenStack gameobject");
            var parent = go.GetParent();
            if (parent == gameObject) { return go; }
            return GetRootFor(parent);
        }

        public GameObject AddScreen(GameObject newScreen) { return gameObject.AddChild(newScreen); }

    }

}
