using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil {

    class TestCoroutines {

        [SetUp]
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestRunningMultipleCoroutines() {

            var runner = new GameObject().GetOrAddComponent<CoroutineRunner>();

            Log.d("Starting parallel coroutines");
            var runningCoroutines = runner.StartCoroutinesInParallel(
                CoroutineA,
                CoroutineA
            );
            Log.d("All parallel coroutines are STARTED now");

            Log.d("Starting sequential coroutines");
            yield return runner.StartCoroutinesSequetially(
                () => CoroutineB(3),
                () => CoroutineB(1)
            );
            Log.d("All sequential coroutines are DONE now");

            // make sure that the parallel started coroutines are all finished before the test ends:
            yield return runningCoroutines.WaitForRunningCoroutinesToFinish();
            Log.d("Now all coroutines (both the parallel and the sequential ones are finished");

        }

        private IEnumerator CoroutineA() {
            var t = Log.MethodEntered();
            yield return new WaitForSeconds(4f);
            Log.MethodDone(t);
        }

        private IEnumerator CoroutineB(float duration) {
            var t = Log.MethodEntered("duration=" + duration);
            yield return new WaitForSeconds(duration);
            Log.MethodDone(t);
        }

    }

}
