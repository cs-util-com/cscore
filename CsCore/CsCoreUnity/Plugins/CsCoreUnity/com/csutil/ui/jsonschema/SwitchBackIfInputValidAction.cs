using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    /// <summary> This action attached to a button and ensures that all RegexValidators of the current screen report that they did
    /// not detect any regex violations in the users input </summary>
    [RequireComponent(typeof(Button))]
    class SwitchBackIfInputValidAction : MonoBehaviour {

        /// <summary> If true the action will be added even if there are already other click listeners registered for the button </summary>
        public bool forceAddingAction = false;
        /// <summary> If true and the final view in the stack is reached then the stack will be destroyed </summary>
        public bool destroyViewStackWhenLastScreenReached = false;
        /// <summary> If true the current view on the stack will be set to inactive instead of destroying it </summary>
        public bool hideNotDestroyCurrentViewWhenGoingBackwards = false;

        private void Start() {
            var b = GetComponent<Button>();
            if (b == null) { throw Log.e("No button found, cant setup automatic switch trigger", gameObject); }
            if (b.onClick.GetPersistentEventCount() == 0 || forceAddingAction) {
                b.AddOnClickAction(delegate {
                    GoBackwards();
                });
            }
        }

        private bool GoBackwards() {
            var vs = gameObject.GetViewStack();
            var currentScreen = vs.GetRootViewOf(gameObject);
            if (!RegexValidator.IsAllInputCurrentlyValid(currentScreen)) {
                return false;
            }
            return vs.SwitchBackToLastView(gameObject, destroyViewStackWhenLastScreenReached, hideNotDestroyCurrentViewWhenGoingBackwards);
        }

    }

}
