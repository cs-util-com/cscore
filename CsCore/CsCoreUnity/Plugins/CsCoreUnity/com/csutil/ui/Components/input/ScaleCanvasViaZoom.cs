using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public class ScaleCanvasViaZoom : MonoBehaviour {

        public EventSystem es;
        public CanvasScalerV2 scaler;
        public float speed = 0.15f;

        private void OnEnable() {
            if (scaler == null) { scaler = GetComponent<CanvasScalerV2>(); }
            if (es == null) { es = EventSystem.current; }
            AssertV2.IsNotNull(scaler, "No CanvasScalerV2 assigned");
            AssertV2.IsNotNull(es, "No event system found in scene");
        }

        private void Update() {
            // First check if input is already handled by event system:
            if (es.currentSelectedGameObject == null) { // If not check for pinch/zoom:
                if (Input.touchCount == 2) { scaler.referenceResolution += GetDragDelta() * speed; }
            }
        }

        private static float GetDragDelta() {
            if (Input.touchCount != 2) { return 0; }
            var finger1 = Input.GetTouch(0);
            var finger2 = Input.GetTouch(1);
            var lastFinger1Pos = finger1.position - finger1.deltaPosition;
            var lastFinger2Pos = finger2.position - finger2.deltaPosition;
            var lastFingerDistance = (lastFinger1Pos - lastFinger2Pos).magnitude;
            var currentFingerDistance = (finger1.position - finger2.position).magnitude;
            return lastFingerDistance - currentFingerDistance;
        }

    }

}