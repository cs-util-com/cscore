using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.csutil {

    public class DestroyOnPlay : MonoBehaviour {

        private void OnEnable() {
            gameObject.Destroy();
        }

    }

}
