using UnityEngine;

namespace com.csutil.ui {

    public class SafeAreaResizer : MonoBehaviour {

        private const float MIN = 0.6f;
        private const float ONE = 1.001f;

        private bool refreshing;

        private void OnEnable() { Recalc(); }

        private void OnRectTransformDimensionsChange() { Recalc(); }

        private void OnValidate() { Recalc(); }

        private void Recalc() {
            if (refreshing || !enabled) { return; }
            refreshing = true;
            CalcAnchors();
            refreshing = false;
        }

        private void CalcAnchors() {
            try {
                RectTransform rt = gameObject.GetOrAddComponent<RectTransform>();
                var canvasRect = rt.GetRootCanvas().pixelRect;
                var safeArea = Screen.safeArea;
                Vector2 anchorMin = safeArea.position;
                anchorMin.x /= canvasRect.width;
                anchorMin.y /= canvasRect.height;

                Vector2 anchorMax = safeArea.position + safeArea.size;
                anchorMax.x /= canvasRect.width;
                anchorMax.y /= canvasRect.height;

                //AssertV2.IsTrue(anchorMin.y <= ONE, "anchorMin.y=" + anchorMin.y);
                //AssertV2.IsTrue(anchorMax.y <= ONE, "anchorMax.y=" + anchorMax.y);
                AssertV2.IsTrue(rt.localScale.magnitude > 0, "rt.localScale=" + rt.localScale);
                if (anchorMax.x > ONE || anchorMax.y > ONE) { return; }
                if (anchorMin.x == 0 && anchorMin.y == 0 && (anchorMax.x < MIN || anchorMax.y < MIN)) { return; }

                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.SetPadding(0);
            }
            catch (System.Exception) { }
        }
    }

}