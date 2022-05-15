using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil {

    public static class PointerEventDataExtensions {

        public static Vector3 localPosition(this PointerEventData e, bool ignoreGlobalScale = false) {
            var go = e.pointerEnter;
            if (go == null) { go = e.pointerPress; }
            var rt = go.GetComponent<RectTransform>();
            if (rt != null) {
                var cam = e.pressEventCamera;
                // Check if cam should be used (see https://docs.unity3d.com/ScriptReference/RectTransformUtility.ScreenPointToLocalPointInRectangle.html ):
                if (rt.GetRootCanvas().renderMode == RenderMode.ScreenSpaceOverlay) { cam = null; }
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, cam, out Vector2 res);
                // TODO get rid of ignoreGlobalScale flag (Ui21 & Ui26_circles conflict each other)
                var scale = ignoreGlobalScale ? rt.localScale : rt.lossyScale;
                return rt.rotation * new Vector2(res.x * scale.x, res.y * scale.y);
            } else {
                return go.transform.InverseTransformPoint(e.pointerCurrentRaycast.worldPosition);
            }
        }

        public static bool IsLeftClick(this PointerEventData self) { return self.button == PointerEventData.InputButton.Left; }

        public static bool IsRightClick(this PointerEventData self) { return self.button == PointerEventData.InputButton.Right; }

    }

}