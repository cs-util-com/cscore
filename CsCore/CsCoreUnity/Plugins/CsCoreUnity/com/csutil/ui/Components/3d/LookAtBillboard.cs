using UnityEngine;
using UnityEngine.Serialization;

namespace com.csutil {

    public class LookAtBillboard : MonoBehaviour {

        public Transform targetToLookAt;
        public float lerpSpeed = 0;
        public float lerpOmega = 20;
        private Vector3 _lerpVelocity = Vector3.zero;
        public bool onlyRotateAroundY = false;
        public bool useTransformForwardAsTarget = false;

        private void OnEnable() {
            if (targetToLookAt == null) { targetToLookAt = Camera.main.transform; }
        }

        private void Update() {
            if (useTransformForwardAsTarget) {
                var newForward = targetToLookAt.forward;
                if (onlyRotateAroundY) {
                    newForward = Vector3.Scale(newForward, new Vector3(1, 0, 1)).normalized;
                }
                if (lerpSpeed > 0) {
                    transform.forward = transform.forward.LerpWithVelocity(newForward, ref _lerpVelocity, Time.deltaTime * lerpSpeed, lerpOmega);
                } else {
                    transform.forward = newForward;
                }
            } else {
                if (lerpSpeed > 0) {
                    // Lerp the rotation of the object to face the target rotation
                    var vector = transform.position - targetToLookAt.position;
                    if (onlyRotateAroundY) { vector.y = 0; }
                    var targetRotation = Quaternion.LookRotation(vector);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
                } else {
                    transform.LookAt(targetToLookAt);
                }
            }

        }

    }

}