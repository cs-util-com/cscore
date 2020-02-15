using UnityEngine;

namespace com.csutil.system {

    class PowerConsumptionReducer : MonoBehaviour {

        private const float ACTIVE_FPS = 60;
        private const float IDLE_FPS = 10;
        public AnimationCurve fpsReductionOverTime = AnimationCurve.EaseInOut(0, ACTIVE_FPS, 10, IDLE_FPS);
        public float time;
        public int currentFps;

        private void Start() {
            this.ExecuteRepeated(() => {
                currentFps = (int)fpsReductionOverTime.Evaluate(time);
                ApplicationV2.targetFrameRateV2 = currentFps;
                time += 0.1f;
                return true;
            }, 100);
        }

        // listen to all kinds of changes:
        private void Update() { if (Input.GetMouseButton(0) || (Input.touchCount > 0)) { ExitIdleFps(); } }
        private void OnCanvasGroupChanged() { ExitIdleFps(); }
        private void OnRectTransformDimensionsChange() { ExitIdleFps(); }
        private void OnTransformChildrenChanged() { ExitIdleFps(); }
        private void OnTransformParentChanged() { ExitIdleFps(); }

        private void ExitIdleFps() { time = 0; }

    }

}
