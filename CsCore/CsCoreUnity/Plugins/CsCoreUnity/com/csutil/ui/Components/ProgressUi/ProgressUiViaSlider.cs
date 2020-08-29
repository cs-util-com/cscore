using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.progress {

    public class ProgressUiViaSlider : ProgressUi {

        public Slider progress;

        protected override GameObject GetProgressUiGo() { return progress.gameObject; }

        protected override void SetPercentInUi(double percent) {
            progress.value = (int)percent;
        }

    }

}
