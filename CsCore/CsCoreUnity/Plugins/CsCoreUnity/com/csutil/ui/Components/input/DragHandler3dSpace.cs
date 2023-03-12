using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.csutil.ui {

    public class DragHandler3dSpace : MonoBehaviour, IBeginDragHandler, IDragHandler {

        private Vector3 localDragStartOffsetOnRt;

        private void OnEnable() {
            AssertCamWithPhysicsRaycasterFoundInScene();
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
            localDragStartOffsetOnRt = transform.position - e.pointerCurrentRaycast.worldPosition;
        }

        public void OnDrag(PointerEventData e) {
            if (e.pointerCurrentRaycast.worldPosition == Vector3.zero) { return; }
            var newWorldPos = e.pointerCurrentRaycast.worldPosition + localDragStartOffsetOnRt;
            var dragDistance = (newWorldPos - transform.position).magnitude;
            if (dragDistance <= 0) { return; }
            transform.position = newWorldPos;
        }

    }

}