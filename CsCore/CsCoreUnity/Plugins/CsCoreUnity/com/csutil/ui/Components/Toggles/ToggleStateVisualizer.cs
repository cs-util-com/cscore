using UnityEngine;

namespace com.csutil.ui {

    public class ToggleStateVisualizer : ToggleListener {

        public GameObject targetToShowHide;

        protected override void OnToggleStateChanged(bool toggleIsOn) {
            targetToShowHide.SetActiveV2(toggleIsOn);
        }
        
    }

}