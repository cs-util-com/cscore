using UnityEngine;
using UnityEngine.Serialization;

namespace com.csutil {

    public class LookAtBillboard : MonoBehaviour {

        public Transform targetToLookAt;
        public float lerpSpeed = 0;
        public float lerpOmega = 20;
        private Vector3 _lerpVelocity = Vector3.zero;
        public bool onlyRotateAroundY = false;

        private void OnEnable() {
            if (targetToLookAt == null) { targetToLookAt = Camera.main.transform; }
        }

        private void Update() {
            var newForward = targetToLookAt.forward;
            if (onlyRotateAroundY) {
                newForward = Vector3.Scale(newForward, new Vector3(1, 0, 1)).normalized;
            }
            if (lerpSpeed > 0) {
                transform.forward = transform.forward.LerpWithVelocity(newForward, ref _lerpVelocity, Time.deltaTime * lerpSpeed, lerpOmega);
            } else {
                transform.forward = newForward;
            }
        }

    }

}