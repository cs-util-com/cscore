using System.Collections;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class DespawnAfterXSec : MonoBehaviour {

        public float secondsUntilDespawn = 3;

        private void OnEnable() {
            StartCoroutine(StartTimer());
        }

        IEnumerator StartTimer() {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(secondsUntilDespawn);
            // Calling destroy on a pooled object will return it to the pool:
            gameObject.Destroy();
        }

    }

}