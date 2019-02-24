using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.elements {

    public class DefaultSwitchScreenAction : MonoBehaviour {

        public enum SwitchDirection { backwards, forwards, loadNextScreenViaPrefab }

        public SwitchDirection switchDirection = SwitchDirection.backwards;
        public string nextScreenPrefabName;
        public bool forceAction = false;

        void Start() {
            var b = GetComponent<Button>();
            if (b == null) { throw Log.e("No button found", gameObject); }
            if (b.onClick.GetPersistentEventCount() == 0 || forceAction) {
                b.AddOnClickAction(delegate {
                    if (!SwitchScreen()) { Log.w("Cant switch screen " + switchDirection); }
                });
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