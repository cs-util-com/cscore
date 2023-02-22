using System;
using UnityEngine;

namespace com.csutil {

    public class LookAtBillboard : MonoBehaviour {

        public Transform targetToLookAt;

        private void OnEnable() {
            if (targetToLookAt == null) { targetToLookAt = Camera.main.transform; }
        }

        private void Update() {
            transform.forward = targetToLookAt.forward;
        }

    }

}