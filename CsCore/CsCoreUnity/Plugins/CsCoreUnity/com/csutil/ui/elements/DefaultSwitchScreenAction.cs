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
        public bool destroyScreenStackWhenLastScreenReached = false;

        private void Start() {
            var b = GetComponent<Button>();
            if (b == null) { throw Log.e("No button found", gameObject); }
            if (b.onClick.GetPersistentEventCount() == 0 || forceAddingAction) {
                b.AddOnClickAction(delegate {
                    if (!SwitchScreen()) { CouldNotSwitchScreen(); }
                });
            }
        }

        private void CouldNotSwitchScreen() {
            Log.w("Cant switch screen in direction " + switchDirection);
            var isForwardOrBackward = switchDirection != SwitchDirection.loadNextScreenViaPrefab;
            if (isForwardOrBackward && destroyScreenStackWhenLastScreenReached) {
                ScreenStack.GetScreenStack(gameObject).gameObject.Destroy();
            }
        }

        private bool SwitchScreen() {
            switch (switchDirection) {
                case SwitchDirection.backwards: return ScreenStack.SwitchBackToLastScreen(gameObject);
                case SwitchDirection.forwards: return ScreenStack.SwitchToNextScreen(gameObject);
                case SwitchDirection.loadNextScreenViaPrefab:
                    return ScreenStack.SwitchToScreen(gameObject, nextScreenPrefabName) != null;
                default: return false;
            }
        }

    }

}