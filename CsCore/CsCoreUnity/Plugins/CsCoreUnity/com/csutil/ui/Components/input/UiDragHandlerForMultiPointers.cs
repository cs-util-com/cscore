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
    public class UiDragHandlerForMultiPointers : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

        public OnDragEvent onDrag = new OnDragEvent();

        private RectTransform rt;
        private int currentFingerCount;

        private Vector2 finger1Start;
        private PointerEventData latestFinger1;
        private Vector2 finger2Start;
        private PointerEventData latestFinger2;

        private Vector2 startDist;
        private Vector3 startLocalScale;

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

        public void OnBeginDrag(PointerEventData e) {
            Log.MethodEnteredWith(e.pointerId);
            rt = transform as RectTransform;
            currentFingerCount++;
            if (rt.GetLocalPointOnRt(e, out Vector2 r)) {
                if (currentFingerCount == 1) {
                    finger1Start = r;
                } else if (currentFingerCount == 2) {
                    finger2Start = r;
                    startLocalScale = rt.localScale;
                    startDist = finger2Start - finger1Start;
                }
            }
        }

        public void OnDrag(PointerEventData e) {
            if (e.pointerId == 0) { latestFinger1 = e; }
            if (e.pointerId == 1) { latestFinger2 = e; }
            if (currentFingerCount != 2) { return; }

            Vector2 currentDist = latestFinger1.position - latestFinger2.position;
            float scaleFactor = currentDist.magnitude / startDist.magnitude;

            var newLocalScale = startLocalScale * scaleFactor;
            var p1 = latestFinger1.position - finger1Start * scaleFactor;
            var p2 = latestFinger2.position - finger2Start * scaleFactor;
            var newPosition = (p1 + p2) / 2f;
            var newLocalRotation = Quaternion.FromToRotation(startDist, currentDist);

            onDrag?.Invoke(newPosition, newLocalScale, newLocalRotation);
        }

        public void OnEndDrag(PointerEventData eventData) {
            currentFingerCount = 0;
            latestFinger1 = null;
            latestFinger2 = null;
        }

        [System.Serializable]
        public class OnDragEvent : UnityEvent<Vector2, Vector3, Quaternion> { }

    }

}