using com.csutil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.csutil.animations {

    public class MoveAlongPath : MonoBehaviour {

        public AnimationCurve moveAnim = NewDefaultAnimCurve();
        public AnimationCurve rotateAnim = NewDefaultAnimCurve();
        public float moveSpeed = 3;
        public float rotateSpeed = 70;

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
                    runningMove = StartCoroutine(targetToMove.MoveTo(_waypointToMoveTo, moveSpeed, moveAnim));
                    if (runningRotate != null) { StopCoroutine(runningRotate); }
                    runningRotate = StartCoroutine(targetToMove.RotateTo(_waypointToMoveTo, rotateSpeed, rotateAnim));
                }
            }
        }

        private bool loopWaypoints = true;

        private static AnimationCurve NewDefaultAnimCurve() {
            var a = AnimationCurve.EaseInOut(0, 0, 1, 1);
            a.preWrapMode = WrapMode.Clamp;
            a.postWrapMode = WrapMode.Clamp;
            return a;
        }

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