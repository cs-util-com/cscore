using System.Linq;
using com.csutil.math;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    /// <summary> Allows dragging objects in 3D space </summary>
    public class DragHandler3dSpace : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

        public Transform targetToDrag;
        public bool keepDistanceToCam = true;
        public bool keepRelativeRotation = false;

        private Vector3 _localDragStartOffsetOnRt;
        private float _distanceAtDragStart;
        private Quaternion _relativeRotation;
        private PointerEventData _latestPointerEventData;

        private FixedSizedQueue<Vector3> positionDeltaHistory = new FixedSizedQueue<Vector3>(3);

        private void Start() {
            AssertCamWithPhysicsRaycasterFoundInScene();
            // If no target is set explicitly then use the transform of this gameobject:
            if (targetToDrag == null) { targetToDrag = transform; }
        }

        private static void AssertCamWithPhysicsRaycasterFoundInScene() {
            var cams = FindObjectsOfType<Camera>();
            if (cams.IsNullOrEmpty()) {
                Log.e("No camera found, raycasts will not work");
            } else {
                var physicsCams = cams.Filter(c => c.GetComponentV2<PhysicsRaycaster>() != null);
                if (physicsCams.IsNullOrEmpty()) {
                    Log.e("No camera withPhysicsRaycaster, raycasts will not work", cams.First());
                }
            }
        }

        public void OnBeginDrag(PointerEventData e) {
            var targetPosition = targetToDrag.position;
            _localDragStartOffsetOnRt = targetPosition - e.pointerCurrentRaycast.worldPosition;
            var cam = e.pressEventCamera.transform;
            _distanceAtDragStart = (cam.position - targetPosition).magnitude;
            AssertV3.IsFalse(_distanceAtDragStart == 0, () => "targetToDrag is at the same position as the camera");
            _relativeRotation = Quaternion.Inverse(cam.rotation) * targetToDrag.rotation;
            _latestPointerEventData = e;
        }

        public void OnDrag(PointerEventData e) {
            _latestPointerEventData = e;
            if (keepDistanceToCam) {
                if (e.pointerCurrentRaycast.worldPosition == Vector3.zero) { return; }
                var newWorldPos = e.pointerCurrentRaycast.worldPosition + _localDragStartOffsetOnRt;
                var camPos = e.pressEventCamera.transform.position;
                var direction = (newWorldPos - camPos).normalized;
                var newPosition = camPos + direction * _distanceAtDragStart;
                var newDelta = newPosition - targetToDrag.position;
                positionDeltaHistory.Enqueue(newDelta);
                var mean = positionDeltaHistory.CalcMean(x => x.sqrMagnitude);
                if (newDelta.sqrMagnitude < mean * 2) {
                    targetToDrag.position = newPosition;
                }
            } else {
                if (e.pointerCurrentRaycast.worldPosition == Vector3.zero) { return; }
                var newWorldPos = e.pointerCurrentRaycast.worldPosition + _localDragStartOffsetOnRt;
                var newDelta = newWorldPos - targetToDrag.position;
                positionDeltaHistory.Enqueue(newDelta);
                var mean = positionDeltaHistory.CalcMean(x => x.sqrMagnitude);
                if (newDelta.sqrMagnitude < mean * 2) {
                    targetToDrag.position = newWorldPos;
                }
            }
            if (keepRelativeRotation) {
                targetToDrag.rotation = e.pressEventCamera.transform.rotation * _relativeRotation;
            }
        }

        public void OnEndDrag(PointerEventData eventData) {
            _latestPointerEventData = null;
        }

        public void Update() {
            if (_latestPointerEventData != null && _latestPointerEventData.dragging) { OnDrag(_latestPointerEventData); }
        }

    }

}