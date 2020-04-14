using UnityEngine;

namespace com.csutil.tests {

    public class SpinRotate : MonoBehaviour {

        public Vector3 axis = Vector3.up;
        public float speed = 10;

        void Update() { transform.Rotate(axis, speed * Time.deltaTime); }

    }

}
