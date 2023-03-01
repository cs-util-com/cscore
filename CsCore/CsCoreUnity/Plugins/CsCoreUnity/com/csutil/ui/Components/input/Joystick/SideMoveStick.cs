using UnityEngine;

namespace com.csutil.ui {

    [RequireComponent(typeof(JoystickUi))]
    public class SideMoveStick : MonoBehaviour {

        public Rigidbody target;
        public float upDownDamp = 0.5f;
        public float speed = 100f;
        /// <summary> If true the up down in the UI will be applied to the up/down (z axis) of the target </summary>
        public bool isUpDownStick = true;

        private bool isCurrentlyDragging = false;
        private Vector2 delta;

        private void OnEnable() {
            if (target == null) { target = Camera.main?.GetComponentV2<Rigidbody>(); }
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
                var y = isUpDownStick ? delta.y * upDownDamp : 0;
                var z = isUpDownStick ? 0 : delta.y * upDownDamp;
                target.AddRelativeForce(new Vector3(delta.x, y, z) * Time.deltaTime * speed);
            }
        }

    }

}