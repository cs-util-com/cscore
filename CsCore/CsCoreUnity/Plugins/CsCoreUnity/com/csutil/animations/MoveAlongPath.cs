using com.csutil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.csutil.animations {

    public class MoveAlongPath : MonoBehaviour {

        public Transform targetToMove;
        public Transform waypointToMoveTo;
        public AnimationCurve moveAnim = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve rotateAnim = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float moveSpeed = 1;
        public float rotateSpeed = 1;

        private Vector3 startPos;
        private Quaternion startRot;
        private bool loop = true;

        public void MoveTargetToNextWaypoint() {
            if (waypointToMoveTo == null) {
                waypointToMoveTo = NextChild(this.transform, -1, loop);
            } else {
                waypointToMoveTo = NextChild(waypointToMoveTo.parent, waypointToMoveTo.GetSiblingIndex(), loop);
            }
            startPos = targetToMove.position;
            startRot = targetToMove.rotation;
        }

        private static Transform NextChild(Transform parent, int currentIndex, bool loop) {
            AssertV2.AreNotEqual(0, parent.childCount);
            if (currentIndex + 1 >= parent.childCount) { return loop ? parent.GetChild(0) : null; }
            return parent.GetChild(currentIndex + 1);
        }

        private void Update() {
            if (waypointToMoveTo == null || targetToMove == null) { return; }
            if (startPos == null) { startPos = targetToMove.position; }
            if (startRot == null) { startRot = targetToMove.rotation; }
            {
                var fullDistance = waypointToMoveTo.position - startPos;
                var traveledDistance = targetToMove.position - startPos;
                var d = traveledDistance.magnitude / fullDistance.magnitude;
                AssertV2.IsTrue(d > 0, "d=" + d, "traveledDistance=" + traveledDistance, "fullDistance=" + fullDistance, "startPos=" + startPos, "waypointToMoveTo.position=" + waypointToMoveTo.position);
                AssertV2.IsTrue(d < 1, "d=" + d, "traveledDistance=" + traveledDistance, "fullDistance=" + fullDistance, "startPos=" + startPos, "waypointToMoveTo.position=" + waypointToMoveTo.position);
                d = moveAnim.Evaluate(Mathf.Clamp(d, 0, 1));
                AssertV2.IsFalse(d == 0, "d==0 so nothing will move!");
                d *= moveSpeed * Time.deltaTime;
                targetToMove.position = Vector3.Lerp(startPos, waypointToMoveTo.position, d);
            }
            {
                var fullRot = Quaternion.Angle(waypointToMoveTo.rotation, startRot);
                var traveledRot = Quaternion.Angle(targetToMove.rotation, startRot);
                var d = rotateAnim.Evaluate(Mathf.Clamp(traveledRot / fullRot, 0, 1));
                d *= rotateSpeed * Time.deltaTime;
                targetToMove.rotation = Quaternion.Lerp(startRot, waypointToMoveTo.rotation, d);
            }
        }

    }

}