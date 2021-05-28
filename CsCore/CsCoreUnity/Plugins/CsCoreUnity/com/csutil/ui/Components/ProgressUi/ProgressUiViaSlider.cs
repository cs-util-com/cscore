using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.progress {

    public class ProgressUiViaSlider : ProgressUi {

        public Slider progress;

        protected override GameObject GetProgressUiGo() { return progress.gameObject; }

        protected override void UpdateUiPercentValue(double percent) {
            if (progress.maxValue != 100) { progress.maxValue = 100; }
            progress.value = (int)percent;
        }

    }

}
