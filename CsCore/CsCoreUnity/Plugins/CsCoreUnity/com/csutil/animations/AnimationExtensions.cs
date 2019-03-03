using UnityEngine;
using System.Collections;

namespace com.csutil {

    public static class AnimationExtensions {

        public static IEnumerator MoveTo(this Transform self, Transform target, float moveSpeed, float omega = 20) {
            var currentVelocity = new Vector3();
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while (true) {
                self.position = self.position.LerpWithVelocity(target.position, ref currentVelocity, Time.deltaTime * moveSpeed, omega);
                yield return waitForEndOfFrame;
            }
        }

        public static IEnumerator MoveTo(this Transform self, Transform target, float moveSpeed, AnimationCurve curve) {
            var start = self.position;
            var waitForEndOfFrame = new WaitForEndOfFrame();
            float currentTime = 0;
            float percent = 0;
            do {
                var fullDistance = (target.position - start).magnitude;
                var fullDuration = fullDistance / moveSpeed; // time = distance / speed
                currentTime += Time.deltaTime;
                percent = currentTime / fullDuration;
                self.position = Vector3.LerpUnclamped(start, target.position, curve.Evaluate(percent));
                yield return waitForEndOfFrame;
            } while (percent < 1);
        }

        public static IEnumerator RotateTo(this Transform self, Transform target, float rotateSpeed, float omega = 1) {
            var currentVelocity = new Vector4();
            var waitForEndOfFrame = new WaitForEndOfFrame();
            while (true) {
                self.rotation = self.rotation.LerpWithVelocity(target.rotation, ref currentVelocity, Time.deltaTime * rotateSpeed, omega);
                yield return waitForEndOfFrame;
            }
        }

        public static IEnumerator RotateTo(this Transform self, Transform target, float rotateSpeed, AnimationCurve curve) {
            var start = self.rotation;
            var waitForEndOfFrame = new WaitForEndOfFrame();
            float currentTime = 0;
            float percent = 0;
            do {
                var fullDistance = Quaternion.Angle(target.rotation, start);
                var fullDuration = fullDistance / rotateSpeed; // time = distance / speed
                currentTime += Time.deltaTime;
                percent = currentTime / fullDuration;
                self.rotation = Quaternion.LerpUnclamped(start, target.rotation, curve.Evaluate(percent));
                yield return waitForEndOfFrame;
            } while (percent < 1);
        }

    }

}