using UnityEngine;

namespace com.csutil.tests {

    public class MyExampleMono2 : MonoBehaviour, IDisposableV2 {

        public DisposeState IsDisposed => DisposeStateHelper.FromBool(this.IsDestroyed());

        private void OnEnable() { Log.d("MyExampleMono2 - OnEnable"); }

        private void OnDisable() { Log.d("MyExampleMono2 - OnDisable"); }

        public void Dispose() { this.gameObject.Destroy(); }
        
    }

}