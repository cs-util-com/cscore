using UnityEngine;
using System.Collections;
using System.Linq;

namespace com.csutil {

    public static class AnimationExtensions {

        public static AnimationCurve CreateCopyWithModifiedTime(this AnimationCurve source, float timeFactorToMultiply) {
            var keys = source.keys.Select(key => {
                float newTime = key.time * timeFactorToMultiply;
                float newInTangent = key.inTangent / timeFactorToMultiply;
                float newOutTangent = key.outTangent / timeFactorToMultiply;
                if (key.weightedMode != WeightedMode.None) {
                    return new Keyframe(newTime, key.value, newInTangent, newOutTangent, key.inWeight, key.outWeight);
                } else {
                    return new Keyframe(newTime, key.value, newInTangent, newOutTangent);
                }
            }).ToArray();
            return new AnimationCurve(keys);
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