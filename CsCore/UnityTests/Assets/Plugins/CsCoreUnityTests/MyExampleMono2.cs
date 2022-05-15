using UnityEngine;

namespace com.csutil.tests {

    public class MyExampleMono2 : MonoBehaviour, IsDisposable {

        public IsDisposable.State IsDisposed => IsDisposable.StateFromBool(this.IsDestroyed());

        private void OnEnable() { Log.d("MyExampleMono2 - OnEnable"); }

        private void OnDisable() { Log.d("MyExampleMono2 - OnDisable"); }

    }

}