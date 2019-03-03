using com.csutil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.animations {

    public class MoveAlongVelocityPath : MonoBehaviour {

        public float moveOmega = 1;
        public float rotateOmega = 20;

        public float moveSpeed = 1;
        public float rotateSpeed = 1;

        private Coroutine runningMove;
        private Coroutine runningRotate;

        public Transform targetToMove;
        private Transform _waypointToMoveTo;
        public Transform waypointToMoveTo {
            get { return _waypointToMoveTo; }
            set {
                if (_waypointToMoveTo != value) {
                    Log.d("new waypointToMoveTo=" + value);
                    _waypointToMoveTo = value;
                    if (runningMove != null) { StopCoroutine(runningMove); }
                    runningMove = StartCoroutine(targetToMove.MoveTo(_waypointToMoveTo, moveSpeed, moveOmega));
                    if (runningRotate != null) { StopCoroutine(runningRotate); }
                    runningRotate = StartCoroutine(targetToMove.RotateTo(_waypointToMoveTo, rotateSpeed, rotateOmega));
                }
            }
        }

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

    }

}