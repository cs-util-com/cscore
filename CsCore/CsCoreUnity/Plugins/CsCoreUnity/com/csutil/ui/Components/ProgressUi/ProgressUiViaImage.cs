using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.progress {

    public class ProgressUiViaImage : ProgressUi {

        public Image progress;
        public bool useAnimations = true;
        public float fadeSpeed = 6;

        private float targetVal;
        private float currentVelocity;

        protected override GameObject GetProgressUiGo() { return progress?.gameObject; }

        protected override void UpdateUiPercentValue(double percent) {
            targetVal = (float)percent / 100f;
            if (!useAnimations) { progress.fillAmount = targetVal; }
        }

        private void Update() {
            if (useAnimations) {
                progress.fillAmount = VelocityLerp.LerpWithVelocity(progress.fillAmount, targetVal, ref currentVelocity, Time.deltaTime * fadeSpeed);
            }
        }

    }

}