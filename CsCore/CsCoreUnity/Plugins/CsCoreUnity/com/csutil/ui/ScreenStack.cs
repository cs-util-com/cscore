using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.ui {

    public class ScreenStack : MonoBehaviour {

        public static Canvas GetUiScreen(GameObject gameObject) {
            var c = gameObject.GetComponentInParent<Canvas>();
            return c != null ? c.rootCanvas : gameObject.GetComponent<Canvas>();
        }

        public static ScreenStack GetUiScreenStack(GameObject gameObject) {
            return gameObject.GetComponentInParent<ScreenStack>();
        }

        public static GameObject SwitchToScreen(GameObject gameObject, string prefabName, bool hideCurrentScreen = true) {
            return SwitchToScreen(gameObject, ResourcesV2.LoadPrefab(prefabName), hideCurrentScreen);
        }

        public static GameObject SwitchToScreen(GameObject gameObject, GameObject newScreen, bool hideCurrentScreen = true) {
            Canvas oldScreen = GetUiScreen(gameObject);
            var op = GetUiScreenStack(gameObject).AddUiScreen(newScreen);
            //op.GetComponent<Canvas>().CopyCanvasSettingsFrom(oldScreen);
            if (hideCurrentScreen) { oldScreen.gameObject.SetActive(false); }
            return op;
        }

        public static void SwitchBackToLastScreen(GameObject gameObject) {
            var screenToClose = GetUiScreen(gameObject);
            var screenStack = GetUiScreenStack(gameObject);
            var l = screenStack.GetAllScreens().Reverse().SkipWhile(c => c != screenToClose);
            AssertV2.IsTrue(l.First() == screenToClose, "screenToClose was not on top");
            var screenToShow = l.Skip(1).FirstOrDefault();
            screenToShow.gameObject.SetActive(true);
            screenToClose.DestroyUiScreen();
        }

        private IEnumerable<Canvas> GetAllScreens() {
            return GetComponentsInChildren<Canvas>(true).Filter(c => c.isRootCanvas);
        }

        public GameObject AddUiScreen(GameObject newScreen) { return gameObject.AddChild(newScreen); }

    }

}
