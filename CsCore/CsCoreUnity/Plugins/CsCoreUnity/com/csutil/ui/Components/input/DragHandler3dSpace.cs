using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    /// <summary> Allows dragging objects in 3D space </summary>
    [RequireComponent(typeof(Collider))]
    public class DragHandler3dSpace : MonoBehaviour, IBeginDragHandler, IDragHandler {

        public Transform targetToDrag;
        public bool keepDistanceToCam = true;

        private Vector3 localDragStartOffsetOnRt;
        private float distanceAtDragStart;

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
            localDragStartOffsetOnRt = targetToDrag.position - e.pointerCurrentRaycast.worldPosition;
            distanceAtDragStart = (e.pressEventCamera.transform.position - targetToDrag.position).magnitude;
        }

        public void OnDrag(PointerEventData e) {
            if (e.pointerCurrentRaycast.worldPosition == Vector3.zero) { return; }
            var newWorldPos = e.pointerCurrentRaycast.worldPosition + localDragStartOffsetOnRt;
            if (keepDistanceToCam) {
                var camPos = e.pressEventCamera.transform.position;
                var direction = (newWorldPos - camPos).normalized;
                targetToDrag.position = camPos + direction * distanceAtDragStart;
            } else {
                targetToDrag.position = newWorldPos;
            }
        }

    }

}