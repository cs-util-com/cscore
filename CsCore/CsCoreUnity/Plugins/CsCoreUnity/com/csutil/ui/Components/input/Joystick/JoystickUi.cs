using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public class JoystickUi : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler {

        /// <summary> Fires when the delta value changes, so not every frame </summary>
        public JoyStickEvent onJoystickChanged = new JoyStickEvent();

        public bool isDragging;
        public float maxDist = 0;
        public RectTransform joystickCenterImg;
        public RectTransform joystickImg;

        private RectTransform _rt;
        private int fingerId;
        private Vector3 centerResetPos;
        private Vector3 joystickResetPos;

        private Vector2 startPos;
        private Vector2 currentPos;
        private Vector2 absDelta;

        public float speed = 10;

        private RectTransform GetRt() {
            if (_rt == null) { _rt = transform as RectTransform; }
            return _rt;
        }

        private void OnValidate() {
            if (joystickCenterImg == null && GetRt().childCount >= 1) {
                joystickCenterImg = GetRt().GetChild(0) as RectTransform;
            }
            if (joystickImg == null && joystickCenterImg != null && joystickCenterImg.childCount >= 1) {
                joystickImg = joystickCenterImg.GetChild(0) as RectTransform;
            }
            if (joystickCenterImg != null && maxDist <= 0) {
                var size = joystickCenterImg.GetSize();
                maxDist = (size.x > size.y ? size.x : size.y) / 2;
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            isDragging = true;
            fingerId = eventData.pointerId;
            startPos = eventData.position;
            centerResetPos = joystickCenterImg.position;
            joystickResetPos = joystickImg.position;
            joystickCenterImg.position = startPos;
            OnDrag(eventData); // To set also the position of the joystickImage
        }

        public void OnDrag(PointerEventData eventData) {
            if (eventData.pointerId != fingerId) { return; }
            currentPos = eventData.position;
            absDelta = currentPos - startPos;
            if (absDelta.magnitude > maxDist) { absDelta = absDelta.normalized * maxDist; }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(GetRt(), startPos + absDelta, null, out Vector2 localP)) {
                joystickImg.position = GetRt().TransformPoint(localP);
            }
            UpdateListeners();
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (eventData.pointerId != fingerId) { return; }
            startPos = Vector2.zero;
            currentPos = Vector2.zero;
            absDelta = Vector2.zero;
            joystickCenterImg.position = centerResetPos;
            joystickImg.position = joystickResetPos;
            isDragging = false;
            UpdateListeners();
        }

        private void UpdateListeners() {
            if (onJoystickChanged != null) { onJoystickChanged.Invoke(isDragging, GetCurrentJoystickDelta()); }
        }

        public Vector2 GetCurrentJoystickDelta() {
            return new Vector2(absDelta.x / maxDist, absDelta.y / maxDist);
        }

        [System.Serializable]
        public class JoyStickEvent : UnityEvent<bool, Vector2> { }

    }

}