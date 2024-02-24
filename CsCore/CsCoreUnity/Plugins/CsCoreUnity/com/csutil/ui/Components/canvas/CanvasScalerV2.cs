using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    [RequireComponent(typeof(CanvasScaler))]
    public class CanvasScalerV2 : MonoBehaviour {

        public float referenceResolution = 700;

        private CanvasScaler canvasScaler;

        private void OnValidate() { UpdateScaler(); } // Called in editor

        public void Start() {
            AssertV3.IsTrue(gameObject.GetComponentV2<Canvas>().isRootCanvasV2(), () => $"{nameof(CanvasScalerV2)} should be added on the root canvas level!", gameObject);
            this.ExecuteRepeated(UpdateScaler, 100);
        }

        public bool UpdateScaler() {
            if (canvasScaler == null) { LazyInitCanvasScaler(); }
            if (referenceResolution > 0) {
                canvasScaler.referenceResolution = new Vector2(referenceResolution, referenceResolution);
            }
            return true;
        }

        private void LazyInitCanvasScaler() {
            canvasScaler = gameObject.GetComponentV2<CanvasScaler>();
            if (canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize) {
                Log.w("Fixed that CanvasScaler not set to ScaleWithScreenSize! Old scaleMode=" + canvasScaler.uiScaleMode);
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            }
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
    }

}