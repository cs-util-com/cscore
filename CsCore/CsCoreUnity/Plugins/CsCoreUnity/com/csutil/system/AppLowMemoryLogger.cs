using UnityEngine;

namespace com.csutil.system {

    class AppLowMemoryLogger : MonoBehaviour {

        private void OnEnable() { Application.lowMemory += LogLowMemory; }

        private void OnDisable() { Application.lowMemory -= LogLowMemory; }

        private void LogLowMemory() { AppFlow.TrackEvent(EventConsts.catSystem, "System reports: Low memory warning!"); }

    }

}