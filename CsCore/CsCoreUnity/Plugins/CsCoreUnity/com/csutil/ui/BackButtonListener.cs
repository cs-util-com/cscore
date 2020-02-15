using System.Linq;
using UnityEngine;

namespace com.csutil.ui {

    /// <summary> Can be attached next to a ViewStack to react to escape/back button clicks </summary>
    class BackButtonListener : MonoBehaviour {

        public bool destroyFinalView = true;

        public void Update() {
            if (Input.GetKeyUp(KeyCode.Escape)) { // back button pressed
                var c = GetComponentInParent<Canvas>()?.rootCanvas;
                if (c != null && c == CanvasFinder.GetAllRootCanvases().First()) {
                    gameObject.GetComponentInParent<ViewStack>()?.SwitchBackToLastView(gameObject, destroyFinalView);
                }
            }
        }

    }

}