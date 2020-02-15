using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.threading {

    class TaskTests {

        [UnityTest]
        public IEnumerator TestRunTaskInBackground() {

            var task1WasStarted = false;
            var task1ReachedItsLoopEnd = false;
            var task1 = TaskRunner.instance.RunInBackground(async (cancelRequest) => {
                task1WasStarted = true;
                Log.d("Task 1 started");
                for (int i = 0; i < 5; i++) {
                    cancelRequest.ThrowIfCancellationRequested();
                    await TaskV2.Delay(200);
                    Log.d("Task 1: Step " + i);
                }
                Log.d("Task 1 loop done, will throw error now..");
                task1ReachedItsLoopEnd = true;
                throw new MyException1("Now the task will fault");
            }).task;
            var errorWasThrown = false;
            yield return task1.AsCoroutine((e) => {
                Assert.IsTrue(e.GetBaseException() is MyException1, "e=" + e.GetBaseException().GetType());
                errorWasThrown = true;
            });
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(errorWasThrown);
            Assert.IsTrue(task1.IsFaulted);
            Assert.IsTrue(task1.Exception.GetBaseException() is MyException1);

            var task2 = TaskRunner.instance.RunInBackground(async (cancelRequest) => {
                Log.d("Task 2 started");
                await TaskV2.Delay(10);
                Assert.IsTrue(task1WasStarted);
                Assert.IsTrue(task1ReachedItsLoopEnd);
                for (int i = 0; i < 5; i++) {
                    cancelRequest.ThrowIfCancellationRequested();
                    await TaskV2.Delay(200);
                    Log.d("Task 2: Step " + i);
                }
                Log.d("Task 2 is done now");
            }).task;
            yield return task2.AsCoroutine();
            Assert.IsTrue(task2.IsCompleted);
            Assert.IsFalse(task2.IsFaulted);
        }

        [UnityTest]
        public IEnumerator TestMainThread() {
            GameObject go = null;
            var task = TaskRunner.instance.RunInBackground(async (cancel) => {
                cancel.ThrowIfCancellationRequested();
                // Test that its not be possible to create a GO in a background thread:
                AssertV2.Throws<Exception>(() => { go = new GameObject(name: "A"); });
                // Test that on MainThread the gameobject can be created:
                MainThread.Invoke(() => { go = new GameObject(name: "B"); });
                await TaskV2.Delay(1000); // wait for main thread action to execute
                Assert.IsTrue(go != null);
                // Assert.AreEqual("B", go.name); // go.name not allowed in background thread
                Log.d("Background thread now done");
            }).task;
            Assert.IsNull(go);
            yield return task.AsCoroutine();
            task.ThrowIfException();
            Assert.IsNotNull(go);
            Assert.AreEqual("B", go.name);
        }

        [UnityTest]
        public IEnumerator TestMainThread2() {
            Assert.IsTrue(MainThread.instance.enabled);
            MainThread.Invoke(() => { Assert.IsTrue(Application.isPlaying); });
            yield return null;
        }

        [Serializable]
        private class MyException1 : Exception { public MyException1(string message) : base(message) { } }
    }

}
