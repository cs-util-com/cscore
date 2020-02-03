using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.system {

    class PerformanceMonitor : MonoBehaviour {

        private void OnEnable() {
            Application.lowMemory += LogLowMemory;
        }

        private void OnDisable() {
            Application.lowMemory -= LogLowMemory;
        }

        private void LogLowMemory() { AppFlow.TrackEvent(EventConsts.catError, "System reports: Low memory warning!"); }


    }

}
