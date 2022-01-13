using System;
using UnityEngine;

namespace com.csutil {

    public class DisposerMono : MonoBehaviour {

        public IDisposable disposable { get; set; }

        private void OnDestroy() {
            try { disposable?.Dispose(); }
            catch (Exception e) { Log.w("Could not dispose target: " + e); }
        }

    }

}