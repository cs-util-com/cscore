using UnityEngine;

namespace com.csutil.ui {

    /// <summary> Allows to use the joystick to move a transform with respect to the current camera view direction </summary>
    [RequireComponent(typeof(JoystickUi))]
    public class TransformMoveStick : MonoBehaviour {

        public Camera camera;
        public Transform target;
        /// <summary> Left/right (x axis) movement speed </summary>
        public float joystickXAxisSpeed = 1f;
        public float joystickYAxisSpeed = 1f;
        /// <summary> If true the up down in the UI will be applied to the up/down (z axis) of the target, else up down will be used for the y axis </summary>
        public bool isUpDownStick = true;

        private Transform camTransform;
        private bool isCurrentlyDragging = false;
        private Vector2 delta;

        private void OnEnable() {
            camera.ThrowErrorIfNull("camera");
            camTransform = camera.transform;
            gameObject.GetComponentV2<JoystickUi>().onJoystickChanged.AddListener(OnForceChange);
        }

        private void Start() {
            if (target == null) { Log.e("No target transform was set for the joystick!", gameObject); }
        }

        private void OnDisable() {
            gameObject.GetComponentV2<JoystickUi>().onJoystickChanged.RemoveListener(OnForceChange);
        }

        private void OnForceChange(bool isNowDragging, Vector2 newForce) {
            isCurrentlyDragging = isNowDragging;
            delta = newForce;
        }

        private void Update() {
            if (isCurrentlyDragging) {
                // Add left/right direction:
                var dir = camTransform.right * delta.x * joystickXAxisSpeed;
                if (isUpDownStick) {
                    // Add up direction:
                    dir += camTransform.up * delta.y * joystickYAxisSpeed;
                } else {
                    // Add forward direction:
                    dir += camTransform.forward * delta.y * joystickYAxisSpeed;
                }
                target.position += dir * Time.deltaTime;
            }
        }

    }

}