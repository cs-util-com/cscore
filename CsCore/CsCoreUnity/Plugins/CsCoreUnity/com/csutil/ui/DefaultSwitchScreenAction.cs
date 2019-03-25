using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.elements {

    [RequireComponent(typeof(Button))]
    public class DefaultSwitchScreenAction : MonoBehaviour {

        public enum SwitchDirection { backwards, forwards, loadNextScreenViaPrefab }

        public SwitchDirection switchDirection = SwitchDirection.backwards;
        /// <summary> e.g. "uis/MyScreenPrefab1" - The path of the prefab that should be loaded as the next screen </summary>
        public string nextScreenPrefabName;
        /// <summary> If true the action will be added even if there are already other click listeners registered for the button </summary>
        public bool forceAddingAction = false;
        /// <summary> If true and the final screen in the stack is reached then the stack will be destroyed </summary>
        public bool destroyViewStackWhenLastScreenReached = false;
        /// <summary> If false the current active screen will not be hidden and the new one shown on top </summary>
        public bool hideCurrentScreen = true;

        private void Start() {
            var b = GetComponent<Button>();
            if (b == null) { throw Log.e("No button found, cant setup automatic switch trigger", gameObject); }
            if (b.onClick.GetPersistentEventCount() == 0 || forceAddingAction) {
                b.AddOnClickAction(delegate { TriggerSwitchScreen(); });
            }
        }

        public void TriggerSwitchScreen() {
            if (!TrySwitchScreen()) {
                var isForwardOrBackward = switchDirection != SwitchDirection.loadNextScreenViaPrefab;
                if (isForwardOrBackward && destroyViewStackWhenLastScreenReached) {
                    gameObject.GetViewStack().gameObject.Destroy();
                } else { Log.w("Cant switch screen in direction " + switchDirection); }
            }
        }

        private bool TrySwitchScreen() {
            switch (switchDirection) {
                case SwitchDirection.backwards: return gameObject.GetViewStack().SwitchBackToLastView(gameObject, destroyViewStackWhenLastScreenReached);
                case SwitchDirection.forwards: return gameObject.GetViewStack().SwitchToNextView(gameObject, hideCurrentScreen);
                case SwitchDirection.loadNextScreenViaPrefab:
                    return gameObject.GetViewStack().ShowView(gameObject, nextScreenPrefabName, hideCurrentScreen) != null;
                default: return false;
            }
        }

    }

}