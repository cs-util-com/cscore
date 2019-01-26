using UnityEngine;

namespace com.csutil.injection.tests {

    public class MyExampleMono1 : MonoBehaviour {

        private void OnEnable() { Log.d("MyExampleMono1 - OnEnable"); }

        private void OnDisable() { Log.d("MyExampleMono1 - OnDisable"); }

    }

}
