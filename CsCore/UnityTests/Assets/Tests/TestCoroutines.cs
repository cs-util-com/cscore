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
            UnitySetup.instance.setup();
        }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestRunningMultipleCoroutines() {

            var runner = new GameObject().GetOrAddComponent<CoroutineRunner>();
            List<Coroutine> parallelCoroutines;
            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                parallelCoroutines = runner.StartCoroutinesInParallel(tasks);
                AssertV2.AreEqual(tasks.Count, parallelCoroutines.Count);
            }
            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(() => coroutineB(1));
                tasks.Add(() => coroutineB(2));
                tasks.Add(() => coroutineB(3));
                Log.d("Starting " + tasks.Count + " coroutines");
                yield return runner.StartCoroutinesSequetially(tasks);
                Log.d("All " + tasks.Count + " coroutines are done now");
            }

            // make sure that the parallel started coroutines are all finished before the test ends:
            foreach (var c in parallelCoroutines) { yield return c; }

        }

        private IEnumerator coroutineA() {
            var t = Log.MethodEntered();
            yield return new WaitForSeconds(2f);
            Log.MethodDone(t);
        }

        private IEnumerator coroutineB(float duration) {
            var t = Log.MethodEntered("duration=" + duration);
            yield return new WaitForSeconds(duration);
            Log.MethodDone(t);
        }

    }

}
