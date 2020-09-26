using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    /// <summary>
    /// Allows to drag UI elements in a canvas using multiple 
    /// pointers influencing position, scale and rotation
    /// 
    /// Recommendation to test this using 2 fingers via Unity Remote: 
    /// https://docs.unity3d.com/Manual/UnityRemote5.html
    /// </summary>
    public class UiDragHandlerForMultiPointers : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler {

        public OnDragEvent onDrag = new OnDragEvent();

        private RectTransform rt;
        private int currentFingerCount;

        private Vector2 finger1Start;
        private Vector2 finger2Start;
        private Vector2 startDist;
        private Vector3 startLocalScale;
        private Quaternion startRotation;

        private PointerEventData latestFinger1;
        private PointerEventData latestFinger2;

        private void Start() {
            if (onDrag.IsNullOrEmpty()) { onDrag.AddListener(ApplyNoRotation); }
        }

        /// <summary> Will update full transform including position, rotation and scale </summary>
        public void ApplyAll(Vector2 newPosition, Vector3 newLocalScale, Quaternion newLocalRotation) {
            rt.localScale = newLocalScale;
            rt.position = newPosition;
            rt.localRotation = newLocalRotation;
        }

        /// <summary> Will only update position and scale and not allow rotation to change </summary>
        public void ApplyNoRotation(Vector2 newPosition, Vector3 newLocalScale, Quaternion _) {
            rt.localScale = newLocalScale;
            rt.position = newPosition;
        }

        public void OnPointerDown(PointerEventData e) {
            rt = transform as RectTransform;
            currentFingerCount++;
            EventSystem.current?.SetSelectedGameObject(gameObject, e);
            if (rt.GetLocalPointOnRt(e, out Vector2 r)) {
                if (currentFingerCount == 1) {
                    finger1Start = r;
                    latestFinger1 = e;
                } else if (currentFingerCount == 2) {
                    finger2Start = r;
                    latestFinger2 = e;
                    startLocalScale = rt.localScale;
                    startRotation = rt.localRotation;
                    startDist = finger2Start - finger1Start;
                }
            }
        }

        public void OnDrag(PointerEventData e) {
            if (e.pointerId == 0) { latestFinger1 = e; }
            if (e.pointerId == 1) { latestFinger2 = e; }
            if (currentFingerCount != 2) { return; }

            Vector2 currentDist = latestFinger2.position - latestFinger1.position;
            float scaleFactor = currentDist.magnitude / startDist.magnitude;
            if (scaleFactor == 0) { return; } // Cancel if there is no diff 

            var newLocalScale = startLocalScale * scaleFactor;
            var p1 = latestFinger1.position - finger1Start * scaleFactor;
            var p2 = latestFinger2.position - finger2Start * scaleFactor;
            var newPosition = (p1 + p2) / 2f;
            var newLocalRotation = startRotation * Quaternion.FromToRotation(startDist, currentDist);

            onDrag?.Invoke(newPosition, newLocalScale, newLocalRotation);
        }

        public void OnEndDrag(PointerEventData eventData) {
            currentFingerCount = 0;
            latestFinger1 = null;
            latestFinger2 = null;
        }

        private void OnDrawGizmos() {
            if (currentFingerCount != 2) { return; }
            var size = 100;
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(latestFinger1.position, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(latestFinger2.position, size);
        }

        [System.Serializable]
        public class OnDragEvent : UnityEvent<Vector2, Vector3, Quaternion> { }

    }

}