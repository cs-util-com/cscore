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
            List<Coroutine> runningCoroutines;
            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(CoroutineA);
                tasks.Add(CoroutineA);
                runningCoroutines = runner.StartCoroutinesInParallel(tasks);
                AssertV2.AreEqual(tasks.Count, runningCoroutines.Count);
            }
            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(() => CoroutineB(1));
                tasks.Add(() => CoroutineB(2));
                tasks.Add(() => CoroutineB(1));
                Log.d("Starting " + tasks.Count + " coroutines");
                yield return runner.StartCoroutinesSequetially(tasks);
                Log.d("All " + tasks.Count + " coroutines are done now");
            }

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
