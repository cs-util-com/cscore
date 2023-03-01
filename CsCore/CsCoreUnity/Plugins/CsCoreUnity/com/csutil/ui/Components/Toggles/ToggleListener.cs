using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(Toggle))]
    public abstract class ToggleListener : MonoBehaviour {

        private UnityAction<bool> listener;

        private void OnEnable() {
            listener = gameObject.GetComponentV2<Toggle>().AddOnValueChangedAction(toggleIsOn => {
                OnToggleStateChanged(toggleIsOn);
                InformParentToggleGroupListenerIfFound();
                return true;
            }, skipChangesByLogic: false);
            OnToggleStateChanged(gameObject.GetComponentV2<Toggle>().isOn);
            InformParentToggleGroupListenerIfFound();
        }

        protected virtual void InformParentToggleGroupListenerIfFound() {
            gameObject.GetComponentInParents<IToggleGroupListener>()?.OnActiveToggleInGroupChanged();
        }

        private void OnDisable() {
            gameObject.GetComponentV2<Toggle>().onValueChanged.RemoveListener(listener);
        }

        protected abstract void OnToggleStateChanged(bool toggleIsOn);

    }

}