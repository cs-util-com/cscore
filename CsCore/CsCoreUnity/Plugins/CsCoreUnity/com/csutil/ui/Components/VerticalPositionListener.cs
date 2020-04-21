using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui.Components {

    [RequireComponent(typeof(RectTransform))]
    class VerticalPositionListener : MonoBehaviour {

        public float verticalPercent;
        public UnityAction<float> onScreen;

        private RectTransform rt;
        private Camera cachedCam;
        private Vector3[] cachedCorners = new Vector3[4];

        private void OnEnable() {
            rt = gameObject.GetOrAddComponent<RectTransform>();
        }

        private void Update() {
            if (cachedCam == null) { cachedCam = rt.GetRootCanvas()?.worldCamera; }
            verticalPercent = rt.GetVerticalPercentOnScreen(cachedCam, cachedCorners);
            if (0 < verticalPercent && verticalPercent < 1) { onScreen?.Invoke(verticalPercent); }
        }

    }

}
