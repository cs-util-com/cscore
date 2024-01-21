using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public interface IToggleGroupListener {
        void OnActiveToggleInGroupChanged();
    }
    
    [RequireComponent(typeof(ToggleGroup))]
    public abstract class ToggleGroupListener : MonoBehaviour, IToggleGroupListener {

        public IEnumerable<Toggle> activeToggles { get; private set; } = new List<Toggle>();

        /// <summary> Can be changed to define how quick after a new tab was selected the logic should be executed to
        /// select and switch to that selected tab. Typically the default value does not need to be changed </summary>
        public double debounceDelayInMs = 50;
        private Action OnActiveToggleInGroupChangedAction;

        protected virtual void OnEnable() {
            AssertChildrenHaveCorrectMonosAttached();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private void AssertChildrenHaveCorrectMonosAttached() {
            foreach (var t in gameObject.GetComponentV2<ToggleGroup>().ActiveToggles()) {
                AssertV3.IsTrue(t.GetComponentV2<ToggleListener>() != null, () => "Missing ToggleListener MonoBehaviour for child of ToggleGroup", t.gameObject);
                AssertV3.IsTrue(t.GetComponentV2<RadioButton>() != null, () => "Missing RadioButton MonoBehaviour for child of ToggleGroup", t.gameObject);
            }
        }

        public void OnActiveToggleInGroupChanged() {
            if (OnActiveToggleInGroupChangedAction == null) {
                OnActiveToggleInGroupChangedAction = OnActiveToggleInGroupChangedDelayed;
                OnActiveToggleInGroupChangedAction = OnActiveToggleInGroupChangedAction.AsThrottledDebounce(debounceDelayInMs, true);
            }
            OnActiveToggleInGroupChangedAction();
        }

        private void OnActiveToggleInGroupChangedDelayed() {
            AssertChildrenHaveCorrectMonosAttached();
            var newActiveToggles = gameObject.GetComponentV2<ToggleGroup>().ActiveToggles();
            if (!newActiveToggles.SequenceReferencesEqual(activeToggles)) {
                activeToggles = new List<Toggle>(newActiveToggles);
                OnActiveToggleInGroupChanged(activeToggles);
            }
        }

        protected abstract void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles);

    }

}