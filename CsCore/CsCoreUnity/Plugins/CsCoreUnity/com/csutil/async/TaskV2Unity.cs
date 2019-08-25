using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.async {

    class TaskV2Unity : TaskV2 {

        protected override Task DelayTask(int millisecondsDelay) {
            return MainThread.instance.StartCoroutineAsTask(CoroutineDelay(millisecondsDelay));
        }

        private IEnumerator CoroutineDelay(int ms) { yield return new WaitForSeconds(ms / 1000f); }

    }

}
