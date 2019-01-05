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
        public IEnumerator TestExecuteDelayed() {

            var counter = 0;
            {
                MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();
                myMonoBehaviour.ExecuteDelayed(() => {
                    counter++;
                }, delayInSecBeforeExecution: 0.6f);
            }
            { // while the delayed task is running check over time if the counter increases:
                yield return new WaitForSeconds(.5f);
                Assert.AreEqual(0, counter);
                yield return new WaitForSeconds(.2f);
                Assert.AreEqual(1, counter);
                yield return new WaitForSeconds(2f);
                Assert.AreEqual(1, counter);
            }

        }

        [UnityTest]
        public IEnumerator TestExecuteRepeated1() {

            var counter = 0;
            {
                MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();
                myMonoBehaviour.ExecuteRepeated(() => {
                    counter++;
                    return counter < 3; // stop repeated execution once 3 is reached
                }, delayInSecBetweenIterations: 0.3f, delayInSecBeforeFirstExecution: .2f);
            }
            { // while the repeated task is running check over time if the counter increases:
                yield return new WaitForSeconds(0.1f);
                Assert.AreEqual(0, counter);
                yield return new WaitForSeconds(0.2f);
                Assert.AreEqual(1, counter);
                yield return new WaitForSeconds(0.3f);
                Assert.AreEqual(2, counter);
                yield return new WaitForSeconds(0.3f);
                Assert.AreEqual(3, counter);
                yield return new WaitForSeconds(0.3f);
                Assert.AreEqual(3, counter);
            }
        }

        [UnityTest]
        public IEnumerator TestExecuteRepeated2() {

            var counter = 0;
            {
                MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();
                myMonoBehaviour.ExecuteRepeated(() => {
                    counter++;
                    return true; // the function will never tell the loop to stop
                }, delayInSecBetweenIterations: 0.1f, repetitions: 3); // stop repeated execution once 3 is reached
                Assert.AreEqual(1, counter); // no delayInSecBeforeFirstExecution is set to task will instantly be executed
            }
            { // while the repeated task is running check over time if the counter increases:
                yield return new WaitForSeconds(0.15f);
                Assert.AreEqual(2, counter);
                yield return new WaitForSeconds(0.1f);
                Assert.AreEqual(3, counter);
                yield return new WaitForSeconds(0.1f);
                Assert.AreEqual(3, counter); // the counter should only increase 3 times we set repetitions: 3
            }
        }

        private static CoroutineRunner CreateSomeMonoBehaviour() { return new GameObject().GetOrAddComponent<CoroutineRunner>(); }

        [UnityTest]
        public IEnumerator TestRunningMultipleCoroutines() {

            MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();

            Log.d("Starting parallel coroutines..");
            var runningCoroutines = myMonoBehaviour.StartCoroutinesInParallel(
                MyCoroutineA,
                MyCoroutineA // this will be started at the same time as the other task
            );
            Log.d("All parallel coroutines are STARTED now");

            Log.d("Starting sequential coroutines..");
            yield return myMonoBehaviour.StartCoroutinesSequetially(
                () => MyCoroutineB(3),
                () => MyCoroutineB(1) // this will only be started after the first task is done
            );
            Log.d("All sequential coroutines are DONE now");

            // make sure that the parallel started coroutines are all finished before the test ends:
            yield return runningCoroutines.WaitForRunningCoroutinesToFinish();
            Log.d("Now all coroutines (both the parallel and the sequential ones are finished");

        }

        private IEnumerator MyCoroutineA() {
            var t = Log.MethodEntered();
            yield return new WaitForSeconds(4f);
            Log.MethodDone(t);
        }

        private IEnumerator MyCoroutineB(float duration) {
            var t = Log.MethodEntered("duration=" + duration);
            yield return new WaitForSeconds(duration);
            Log.MethodDone(t);
        }

        [UnityTest]
        public IEnumerator TestExecuteDelayed2() {
            {
                MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();
                myMonoBehaviour.enabled = false;
                AssertV2.Throws<Exception>(() => {
                    myMonoBehaviour.ExecuteDelayed(() => { throw Log.e("Executing coroutine of disabled mono"); });
                });
            }
            {
                MonoBehaviour myMonoBehaviour = CreateSomeMonoBehaviour();
                myMonoBehaviour.gameObject.SetActive(false);
                AssertV2.Throws<Exception>(() => {
                    myMonoBehaviour.ExecuteDelayed(() => { throw Log.e("Executing coroutine of inactive GO"); });
                });
            }
            yield return null;
        }

    }

}
