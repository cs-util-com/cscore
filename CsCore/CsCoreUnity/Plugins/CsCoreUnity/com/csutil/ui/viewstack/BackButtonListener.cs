#if !UNITY_2021_2_OR_NEWER
using com.csutil.netstandard2_1polyfill;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.ui.viewstack {

    /// <summary> Can be attached next to a ViewStack to react to escape/back button clicks </summary>
    [RequireComponent(typeof(ViewStack))]
    class BackButtonListener : MonoBehaviour {

        public bool destroyFinalView = true;

        private IUnityInputSystem input;
        
        private void OnEnable() {
            input = InputV2.GetInputSystem();
        }

        public void Update() {
            if (input.GetKeyUp(KeyCode.Escape)) { // back button pressed
                Log.d("Back key pressed");
                var vs = gameObject.GetComponentV2<ViewStack>();
                var rootCanvasOnTop = GetCanvasWithHighestSortingOrder();
                var viewToCloseOfOwnViewStack = vs.GetLatestView();
                if (viewToCloseOfOwnViewStack.IsGrandChildOf(rootCanvasOnTop)) {
                    SwitchBackToLastViewViaBackKey(vs, viewToCloseOfOwnViewStack);
                } else {
                    Log.w("1) The ViewStack of the back button listener is not part of the current root canvas on top, so will not react to the back button press", viewToCloseOfOwnViewStack);
                    Log.w("2) The ViewStack of the back button listener is not part of the current root canvas on top, so will not react to the back button press", rootCanvasOnTop);
                    //SwitchBackToLastViewViaBackKey(vs, screenOnTop);
                }
            }
        }

        private static GameObject GetCanvasWithHighestSortingOrder() {
            return RootCanvas.GetAllRootCanvases().First().gameObject;
        }

        private void SwitchBackToLastViewViaBackKey(ViewStack viewStack, GameObject viewToClose) {
            Log.MethodEnteredWith("ViewStack" + viewStack, "viewToClose=" + viewToClose);
            AssertViewToCloseIsInThisViewStack(viewStack, viewToClose);
            if (!viewStack.SwitchBackToLastView(viewToClose)) {
                // The last view was reached so the switch back could not be performed
                if (destroyFinalView) { viewStack.DestroyViewStack(); }
            }
        }

        private static void AssertViewToCloseIsInThisViewStack(ViewStack viewStack, GameObject viewToClose) {
            try {
                viewStack.GetRootViewOf(viewToClose);
            } catch (Exception e) {
                Log.e("1) The current ViewStack was not a parent of the identified viewToClose=" + viewToClose, e, viewToClose);
                Log.e("2) The current ViewStack was not a parent of the identified viewToClose=" + viewToClose, e, viewStack);
            }
        }

    }

}