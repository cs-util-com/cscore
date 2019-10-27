using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerV2 : MonoBehaviour {

        public float referenceResolution = 0;
        public AnimationCurve matchWidthOrHeight = AnimationCurve.Constant(0, 1, 1);

        private CanvasScaler canvasScaler;

        private void OnValidate() { UpdateScaler(); } // Called in editor

        public void OnEnable() { this.ExecuteRepeated(UpdateScaler, 100); }

        public bool UpdateScaler() {
            if (canvasScaler == null) { canvasScaler = GetComponent<CanvasScaler>(); }
            float ratio = Screen.width / (float)Screen.height;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight.Evaluate(ratio);
            if (referenceResolution > 0) {
                canvasScaler.referenceResolution = new Vector2(referenceResolution, referenceResolution);
            }
            return true;
        }

    }

}