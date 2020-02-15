using com.csutil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.animations {

    public class MoveAlongVelocityPath : MonoBehaviour {

        public float moveOmega = 3;
        public float rotateOmega = 3;

        public float moveSpeed = 1;
        public float rotateSpeed = 1;
        
        public Transform targetToMove;
        public Transform waypointToMoveTo;

        private Vector3 moveVelocity = new Vector3();
        private Vector4 rotateVelocity = new Vector4();

        private bool loopWaypoints = true;

        public void MoveTargetToNextWaypoint() {
            if (waypointToMoveTo == null) {
                waypointToMoveTo = GetNextChild(this.transform, -1, loopWaypoints);
            } else {
                waypointToMoveTo = GetNextChild(waypointToMoveTo.parent, waypointToMoveTo.GetSiblingIndex(), loopWaypoints);
            }
        }

        private static Transform GetNextChild(Transform parent, int currentIndex, bool loopChildren) {
            AssertV2.AreNotEqual(0, parent.childCount);
            if (currentIndex + 1 >= parent.childCount) { return loopChildren ? parent.GetChild(0) : null; }
            return parent.GetChild(currentIndex + 1);
        }

        private void Update() {
            if (targetToMove == null || waypointToMoveTo == null) { return; }
            var t = Time.deltaTime;
            targetToMove.position = targetToMove.position.LerpWithVelocity(waypointToMoveTo.position, ref moveVelocity, t * moveSpeed, moveOmega);
            targetToMove.rotation = targetToMove.rotation.LerpWithVelocity(waypointToMoveTo.rotation, ref rotateVelocity, t * rotateSpeed, rotateOmega);
        }

    }

}