using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil {

    public static class PointerEventDataExtensions {

        public static Vector2 localPosition(this PointerEventData e) {
            var rt = e.pointerEnter.GetComponent<RectTransform>();
            if (rt != null) {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out Vector2 result);
                return result;
            } else {
                return e.pointerEnter.transform.InverseTransformPoint(e.pointerCurrentRaycast.worldPosition);
            }
        }

        public static bool IsLeftClick(this PointerEventData self) { return self.button == PointerEventData.InputButton.Left; }

        public static bool IsRightClick(this PointerEventData self) { return self.button == PointerEventData.InputButton.Right; }

    }

}