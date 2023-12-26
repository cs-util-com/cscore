using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.csutil {
    
    public class OnDestroyMono : MonoBehaviour {

        public UnityEvent onDestroy = new UnityEvent();

        private void OnDestroy() {
            try { onDestroy.Invoke(); } 
            catch (Exception e) { Log.e("Could not dispose target: " + e, e); }
        }

    }
    
}