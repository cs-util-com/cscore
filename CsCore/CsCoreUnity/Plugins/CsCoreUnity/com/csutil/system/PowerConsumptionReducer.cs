using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace com.csutil.system {

    class PowerConsumptionReducer : MonoBehaviour {

        [Obsolete("Not needed anymore, keep empty")]
        public AnimationCurve fpsReductionOverTime = null;

        public float maxFps = 120;
        public float idleFps = 10;
        public float idleFpsTime = 60;
        private AnimationCurve _usedFpsReductionOverTime;

        public float time;
        public int currentFps;
        public bool disableInUnityEditor = true;

        private IUnityInputSystem input;

        private void OnEnable() { UpdateFpsReductionOverTimeCurve(); }

        private void UpdateFpsReductionOverTimeCurve() {
            if (fpsReductionOverTime == null || fpsReductionOverTime.length < 2) {
                float normalFps = ApplicationV2.targetFrameRateV2;
                if (normalFps <= 0) { normalFps = maxFps; }
                _usedFpsReductionOverTime = AnimationCurve.EaseInOut(0, normalFps, idleFpsTime, idleFps);
            } else {
                _usedFpsReductionOverTime = fpsReductionOverTime;
            }
        }

        private void Start() {
            input = InputV2.GetInputSystem();
            this.ExecuteRepeated(() => {
                currentFps = (int)_usedFpsReductionOverTime.Evaluate(time);
                AssertV3.AreNotEqual(0, currentFps, "currentFps");
                if (currentFps != 0) {
                    ApplicationV2.targetFrameRateV2 = currentFps;
                }
                time += 0.1f;
                return true;
            }, 100);

            #if UNITY_EDITOR
            enabled = !disableInUnityEditor;
            #endif
        }

        private void Update() {
            // listen to all kinds of user touch / mouse input:
            if (input.GetMouseButton(button: 0) || (input.touchCount > 0)) {
                ExitIdleFps();
            }
        }

        private void OnCanvasGroupChanged() { ExitIdleFps(); }
        private void OnRectTransformDimensionsChange() { ExitIdleFps(); }
        private void OnTransformChildrenChanged() { ExitIdleFps(); }
        private void OnTransformParentChanged() { ExitIdleFps(); }

        private void ExitIdleFps() {
            time = 0;
            UpdateFpsReductionOverTimeCurve();
        }

    }

}