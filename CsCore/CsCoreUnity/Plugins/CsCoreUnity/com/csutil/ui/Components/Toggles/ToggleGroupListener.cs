using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(ToggleGroup))]
    public abstract class ToggleGroupListener : MonoBehaviour {

        public IEnumerable<Toggle> activeToggles { get; private set; } = new List<Toggle>();

        private void OnEnable() {
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
                OnActiveToggleInGroupChanged(activeToggles);
            }
        }

        protected abstract void OnActiveToggleInGroupChanged(IEnumerable<Toggle> activeToggles);

    }

}