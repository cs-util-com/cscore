using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(Toggle))]
    public abstract class ToggleListener : MonoBehaviour {

        private UnityAction<bool> listener;

        private void OnEnable() {
            listener = GetComponent<Toggle>().AddOnValueChangedAction(toggleIsOn => {
                ShowToggleState(toggleIsOn);
                return true;
            }, skipChangesByLogic: false);
            ShowToggleState(GetComponent<Toggle>().isOn);
        }

        private void OnDisable() {
            GetComponent<Toggle>().onValueChanged.RemoveListener(listener);
        }

        protected abstract void ShowToggleState(bool toggleIsOn);

    }

}