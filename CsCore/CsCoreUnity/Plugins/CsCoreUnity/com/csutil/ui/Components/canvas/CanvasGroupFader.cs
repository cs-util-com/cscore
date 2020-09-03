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
            initialAlpha = GetComponent<CanvasGroup>().alpha;
            targetAlpha = initialAlpha;
            this.ExecuteRepeated(delegate {
                if (canvasGroup == null) { canvasGroup = GetComponent<CanvasGroup>(); }
                var a = canvasGroup.alpha;
                if (a != targetAlpha) {
                    canvasGroup.alpha = VelocityLerp.LerpWithVelocity(a, targetAlpha, ref currentVelocity, Time.deltaTime * fadeSpeed);
                }
                return true;
            }, delayInMsBetweenIterations);
        }

    }

}
