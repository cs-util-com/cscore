using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.progress {

    public class ProgressUiViaImage : ProgressUi {

        public Image progress;

        protected override GameObject GetProgressUiGo() { return progress.gameObject; }

        protected override void SetPercentInUi(double percent) {
            progress.fillAmount = (float)percent / 100f;
        }

    }

}
