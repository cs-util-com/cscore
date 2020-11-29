using UnityEngine;

namespace com.csutil.ui {

    [RequireComponent(typeof(CanvasGroup))]
    public class CanvasGroupFader : MonoBehaviour {

        public float initialAlpha;
        public float targetAlpha;
        public float fadeSpeed = 10;
        public int delayInMsBetweenIterations = 20;

        private CanvasGroup canvasGroup;
        private float currentVelocity;

        private void OnEnable() {
            this.ExecuteRepeated(delegate {
                var currentAlpha = GetCanvasGroup().alpha;
                if (currentAlpha != targetAlpha) {
                    var newAlpha = VelocityLerp.LerpWithVelocity(currentAlpha, targetAlpha, ref currentVelocity, Time.deltaTime * fadeSpeed);
                    canvasGroup.alpha = newAlpha;
                }
                return true;
            }, delayInMsBetweenIterations);
        }

        public CanvasGroup GetCanvasGroup() {
            if (canvasGroup == null) {
                canvasGroup = GetComponent<CanvasGroup>();
                initialAlpha = canvasGroup.alpha;
                targetAlpha = initialAlpha;
            }
            return canvasGroup;
        }

    }

}
