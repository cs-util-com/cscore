using UnityEngine;

namespace com.csutil.ui {

    public class SafeAreaResizer : MonoBehaviour {

        private bool refreshing;

        private void OnRectTransformDimensionsChange() { Recalc(); }

        private void OnValidate() { Recalc(); }

        private void Recalc() {
            if (refreshing) { return; }
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

                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.SetPadding(0);
            }
            catch (System.Exception e) { Log.d("" + e); }
        }
    }

}