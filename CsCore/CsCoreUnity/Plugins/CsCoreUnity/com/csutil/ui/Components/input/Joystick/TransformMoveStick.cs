using UnityEngine;

namespace com.csutil.ui {

    [RequireComponent(typeof(JoystickUi))]
    public class TransformMoveStick : MonoBehaviour {

        public Camera camera;
        public Transform target;
        public float joystickYAxisSpeed = 1f;
        public float joystickXAxisSpeed = 1f;
        /// <summary> If true the up down in the UI will be applied to the up/down (z axis) of the target, else up down will be used for the y axis </summary>
        public bool isUpDownStick = true;

        private bool isCurrentlyDragging = false;
        private Vector2 delta;

        private void OnEnable() {
            camera.ThrowErrorIfNull("camera");
            if (target == null) { target = camera.transform; }
            gameObject.GetComponentV2<JoystickUi>().onJoystickChanged.AddListener(OnForceChange);
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
                var y = isUpDownStick ? delta.y * joystickYAxisSpeed : 0;
                var z = isUpDownStick ? 0 : delta.y * joystickYAxisSpeed;

                var forward = camera.transform.right * delta.x * joystickXAxisSpeed;
                var dir = forward;
                if (isUpDownStick) {
                    var up = camera.transform.up * delta.y * joystickYAxisSpeed;
                    dir += up;
                } else {
                    var right = camera.transform.forward * delta.y * joystickYAxisSpeed;
                    dir += right;
                }
                target.position += dir * Time.deltaTime;
            }
        }

    }

}