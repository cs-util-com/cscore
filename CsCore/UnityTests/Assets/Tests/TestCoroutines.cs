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

            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(coroutineA);
                tasks.Add(coroutineA);
                var coroutines = runner.StartCoroutinesInParallel(tasks);
                AssertV2.AreEqual(2, coroutines.Count);
            }
            {
                List<Func<IEnumerator>> tasks = new List<Func<IEnumerator>>();
                tasks.Add(() => coroutineB(2));
                tasks.Add(() => coroutineB(5));
                tasks.Add(() => coroutineB(7));
                Log.d("Starting " + tasks.Count + " coroutines");
                yield return runner.StartCoroutinesSequetially(tasks);
                Log.d("All " + tasks.Count + " coroutines are done now");
            }

        }

        private IEnumerator coroutineA() {
            var t = Log.MethodEntered();
            yield return new WaitForSeconds(2f);
            Log.MethodDone(t);
        }

        private IEnumerator coroutineB(int x) {
            var t = Log.MethodEntered("x=" + x);
            yield return new WaitForSeconds(2f);
            Log.MethodDone(t);
        }

    }

}
