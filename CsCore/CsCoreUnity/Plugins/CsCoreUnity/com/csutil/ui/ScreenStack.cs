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
            var oldScreen = screenStack.GetRootFor(gameObject);
            var oldIndex = oldScreen.transform.GetSiblingIndex();
            AssertV2.AreEqual(oldIndex, screenStack.transform.childCount - 1, "Current was not last screen in the stack");
            if (oldIndex > 0) { screenStack.transform.GetChild(oldIndex - 1).gameObject.SetActive(true); }
            oldScreen.Destroy();
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
