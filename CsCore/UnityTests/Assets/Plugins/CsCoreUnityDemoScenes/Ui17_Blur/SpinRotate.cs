using UnityEngine;

namespace com.csutil.tests {

    public class SpinRotate : MonoBehaviour {

        public Vector3 axis = Vector3.up;
        public float speed = 10;
        public float randomizeFactor = 0;
        private float _randomOffset = 0;

        private void OnEnable() {
            if (randomizeFactor > 0) {
                _randomOffset = UnityEngine.Random.Range(0, randomizeFactor);
            }
        }

        void Update() {
            transform.Rotate(axis, (speed + _randomOffset) * Time.deltaTime);
        }

    }

}