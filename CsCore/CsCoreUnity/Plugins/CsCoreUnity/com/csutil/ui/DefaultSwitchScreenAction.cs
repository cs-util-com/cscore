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
        /// <summary> e.g. "uis/MyScreenPrefab1" - The path of the prefab that should be loaded as the next view </summary>
        public string nextScreenPrefabName;
        /// <summary> If true the action will be added even if there are already other click listeners registered for the button </summary>
        public bool forceAddingAction = false;
        /// <summary> If true and the final view in the stack is reached then the stack will be destroyed </summary>
        public bool destroyViewStackWhenLastScreenReached = false;
        /// <summary> If false the current active view will not be hidden and the new one shown on top </summary>
        public bool hideCurrentScreen = true;
        /// <summary> If true the current view on the stack will be set to inactive instead of destroying it </summary>
        public bool hideNotDestroyCurrentViewWhenGoingBackwards = false;

        private void Start() {
            var b = GetComponent<Button>();
            if (b == null) { throw Log.e("No button found, cant setup automatic switch trigger", gameObject); }
            if (b.onClick.GetPersistentEventCount() == 0 || forceAddingAction) {
                b.AddOnClickAction(delegate { TriggerSwitchView(); });
            }
        }

        public bool TriggerSwitchView() {
            if (!TrySwitchView()) {
                var isForwardOrBackward = switchDirection != SwitchDirection.loadNextScreenViaPrefab;
                if (isForwardOrBackward && destroyViewStackWhenLastScreenReached) {
                    gameObject.GetViewStack().gameObject.Destroy();
                    return true;
                }
                Log.w("Cant switch screen in direction " + switchDirection);
            }
            AppFlow.TrackEvent(EventConsts.catView, "switchViewWasRejected");
            return false;
        }

        private bool TrySwitchView() {
            switch (switchDirection) {
                case SwitchDirection.backwards:
                    return gameObject.GetViewStack().SwitchBackToLastView(gameObject, destroyViewStackWhenLastScreenReached, hideNotDestroyCurrentViewWhenGoingBackwards);
                case SwitchDirection.forwards:
                    return gameObject.GetViewStack().SwitchToNextView(gameObject, hideCurrentScreen);
                case SwitchDirection.loadNextScreenViaPrefab:
                    return gameObject.GetViewStack().ShowView(nextScreenPrefabName, hideCurrentScreen ? gameObject : null) != null;
                default: return false;
            }
        }

    }

}