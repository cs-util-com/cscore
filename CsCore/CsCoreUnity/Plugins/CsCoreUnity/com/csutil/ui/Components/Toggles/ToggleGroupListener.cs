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
        private Action<IEnumerable<Toggle>> OnActiveToggleInGroupChangedAction;

        protected virtual void OnEnable() {
            AssertChildrenHaveCorrectMonosAttached();
        }

        [Conditional("DEBUG"), Conditional("ENFORCE_ASSERTIONS")]
        private void AssertChildrenHaveCorrectMonosAttached() {
            foreach (var t in GetComponent<ToggleGroup>().ActiveToggles()) {
                AssertV2.IsTrue(t.GetComponentV2<ToggleListener>() != null, "Missing ToggleListener MonoBehaviour for child of ToggleGroup", t.gameObject);
                AssertV2.IsTrue(t.GetComponentV2<RadioButton>() != null, "Missing RadioButton MonoBehaviour for child of ToggleGroup", t.gameObject);
            }
        }

        public void OnActiveToggleInGroupChanged() {
            AssertChildrenHaveCorrectMonosAttached();
            var newActiveToggles = GetComponent<ToggleGroup>().ActiveToggles();
            if (!newActiveToggles.SequenceReferencesEqual(activeToggles)) {
                activeToggles = new List<Toggle>(newActiveToggles);
                HandleActiveToggleInGroupChanged(activeToggles);
            }
        }

        private void HandleActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles) {
            if (OnActiveToggleInGroupChangedAction == null) {
                OnActiveToggleInGroupChangedAction = OnActiveToggleInGroupChanged2;
                OnActiveToggleInGroupChangedAction = OnActiveToggleInGroupChangedAction.AsThrottledDebounce(debounceDelayInMs, true);
            }
            OnActiveToggleInGroupChangedAction(activeToggles);
        }

        protected abstract void OnActiveToggleInGroupChanged2(IEnumerable<Toggle> activeToggles);

    }

}