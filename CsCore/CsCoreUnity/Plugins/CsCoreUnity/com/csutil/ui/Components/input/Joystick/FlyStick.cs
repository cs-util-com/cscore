using UnityEngine;

namespace com.csutil.ui {

    [RequireComponent(typeof(JoystickUi))]
    public class FlyStick : MonoBehaviour {

        public Rigidbody target;
        public AnimationCurve acceleration = AnimationCurve.EaseInOut(0, 0, 1, 1);
        /// <summary> When true will ensure the camera never tilts sideways </summary>
        public bool lockRotationZ = true;
        /// <summary> Double tapping the FlyStick flips the move direction (forwards <-> backwards) </summary>
        public bool allowDirectionSwitching = true;
        /// <summary> When true pressing the FlyStick adds a movement force to the target </summary>
        public bool allowThrustByClick = true;
        /// <summary> Damps the up/down rotation relative to the sideway left/right rotation </summary>
        public float upDownDamp = 0.5f;
        public float doubleClickTimeoutInS = 0.3f;

        private Vector3 direction = Vector3.forward;
        private bool isCurrentlyDragging = false;
        private Vector2 currentForce;
        private float dragStartTime;
        private float moveSpeed;

        private void OnEnable() {
            if (target == null) { target = Camera.main?.GetComponent<Rigidbody>(); }
            GetComponent<JoystickUi>().onJoystickChanged.AddListener(OnForceChange);
        }

        private void OnDisable() {
            GetComponent<JoystickUi>().onJoystickChanged.RemoveListener(OnForceChange);
        }

        private void OnForceChange(bool isNowDragging, Vector2 newForce) {
            if (!isCurrentlyDragging && isNowDragging) {
                if (allowDirectionSwitching && Time.time - dragStartTime < doubleClickTimeoutInS) {
                    direction = -1 * direction; // Invert the current direction
                }
                dragStartTime = Time.time; 
            }
            isCurrentlyDragging = isNowDragging;
            currentForce = newForce;
        }

        private void Update() {
            if (isCurrentlyDragging) {
                if (allowThrustByClick) {
                    moveSpeed = acceleration.Evaluate(Time.time - dragStartTime);
                    target.AddRelativeForce(direction * moveSpeed);
                }
                target.AddRelativeTorque(new Vector3(-currentForce.y * upDownDamp, currentForce.x, 0));
            }
            if (lockRotationZ) {
                var eulerAnges = target.rotation.eulerAngles;
                target.rotation = Quaternion.Euler(eulerAnges.x, eulerAnges.y, 0);
            }
        }

    }

}