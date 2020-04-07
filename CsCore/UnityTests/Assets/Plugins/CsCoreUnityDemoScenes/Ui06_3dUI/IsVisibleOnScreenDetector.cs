using UnityEngine;

namespace com.csutil.tests.ui {

    class IsVisibleOnScreenDetector : MonoBehaviour {

        private void OnBecameVisible() {
            // Triggered when the GameObjects renderer is needed
            // In the editor will also be triggered by scene view camera!
            Log.d(gameObject + " is now visible on the screen", gameObject);
        }

        private void OnBecameInvisible() {
            // Triggered when the GameObjects renderer is not needed (e.g. GO not active or not shown by any camera)
            // In the editor will also be triggered by scene view camera!
            Log.d(gameObject + " is now not visible on the screen", gameObject);
        }

    }

}