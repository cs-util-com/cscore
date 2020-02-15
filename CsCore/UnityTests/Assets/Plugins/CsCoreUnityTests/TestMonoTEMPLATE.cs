using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests {

    [Obsolete("Use the UnitTestMono class instead, it works similar but has some advantages")]
    // This MonoBehaviour can be used for manual tests by attaching it to a scene
    class TestMonoTEMPLATE : MonoBehaviour { IEnumerator Start() { yield return new TestTEMPLATE().Test123(); } }

    // The automated unit test that is called by the MonoBehaviour or by the Unity Test Runner
    class TestTEMPLATE {
        [UnityTest]
        public IEnumerator Test123() {
            yield return null; // TODO The test code goes here
        }
    }

}