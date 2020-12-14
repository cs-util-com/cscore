using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    /// <summary> Allows dragging UI elements in a canvas </summary>
    public class UiDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler {

        public Vector2 localDragStartOffsetOnRt;
        public Vector2 targetPos;
        public OnDragEvent onDrag = new OnDragEvent();

        private void Start() {
            if (onDrag.IsNullOrEmpty()) {
                onDrag.AddListener((targetPos) => transform.position = targetPos);
            }
        }

        public void OnBeginDrag(PointerEventData e) {
            localDragStartOffsetOnRt = e.localPosition();
        }

        public void OnDrag(PointerEventData eventData) {
            targetPos = eventData.position - localDragStartOffsetOnRt;
            onDrag?.Invoke(targetPos);
        }

        [System.Serializable]
        public class OnDragEvent : UnityEvent<Vector2> { }

    }

}