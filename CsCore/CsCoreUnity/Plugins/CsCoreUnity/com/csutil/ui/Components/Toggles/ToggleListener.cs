using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(Toggle))]
    public abstract class ToggleListener : MonoBehaviour {

        private UnityAction<bool> listener;

        private void OnEnable() {
            listener = GetComponent<Toggle>().AddOnValueChangedAction(toggleIsOn => {
                OnToggleStateChanged(toggleIsOn);
                InformParentToggleGroupListenerIfFound();
                return true;
            }, skipChangesByLogic: false);
            InformParentToggleGroupListenerIfFound();
            OnToggleStateChanged(GetComponent<Toggle>().isOn);
        }

        private void InformParentToggleGroupListenerIfFound() {
            gameObject.GetComponentInParents<ToggleGroupListener>()?.OnActiveToggleInGroupChanged();
        }

        private void OnDisable() {
            GetComponent<Toggle>().onValueChanged.RemoveListener(listener);
        }

        protected abstract void OnToggleStateChanged(bool toggleIsOn);

    }

}