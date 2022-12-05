using NUnit.Framework;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil.tests.threading {

    class BackgroundTaskQueueTests {

        [UnityTest]
        public IEnumerator TestMixingBackgroundAndMainThread() {
            var timing = Log.MethodEntered("TestMixingBackgroundAndMainThread");

            var queue = BackgroundTaskQueue.NewBackgroundTaskQueue(1);

            var backgroundTask1IsDone = false;
            yield return queue.Run(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                await TaskV2.Delay(150);
                backgroundTask1IsDone = true;
            }).AsCoroutine();

            Assert.IsTrue(backgroundTask1IsDone);
            Assert.IsTrue(MainThread.isMainThread);

            var backgroundTask2 = queue.Run(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                await TaskV2.Delay(150);
                c.ThrowIfCancellationRequested();
                return "some result";
            });
            yield return backgroundTask2.AsCoroutine();
            Assert.IsTrue(backgroundTask2.IsCompleted);
            Assert.AreEqual("some result", backgroundTask2.Result);

            Assert.IsTrue(timing.ElapsedMilliseconds > 200, "t=" + timing.ElapsedMilliseconds);
            Log.MethodDone(timing);
        }

        [UnityTest]
        public IEnumerator TestTaskCancel1() {
            var timing = Log.MethodEntered("TestTaskCancel");

            BackgroundTaskQueue queue = BackgroundTaskQueue.NewBackgroundTaskQueue(1);

            var counter = 0;
            var task1 = queue.Run(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                for (int i = 0; i < int.MaxValue; i++) {
                    await TaskV2.Delay(10);
                    counter++;
                    c.ThrowIfCancellationRequested();
                }
            });
            yield return new WaitForSeconds(0.2f);
            queue.CancelAllOpenTasks();
            yield return task1.AsCoroutine(onError: e => {
                Assert.True(e is AggregateException a1 && a1.InnerException is AggregateException a2 && a2.InnerException is TaskCanceledException);
            });
            Assert.IsFalse(task1.IsCanceled); // It was already running so it faulted instead of being canceled
            Assert.IsTrue(task1.IsFaulted);
            Assert.IsTrue(counter > 10, "counter=" + counter);
            Assert.IsTrue(counter < 50, "counter=" + counter);

            Log.MethodDone(timing);
        }

        [UnityTest]
        public IEnumerator TestTaskCancel2() {
            var timing = Log.MethodEntered("TestTaskCancel");

            GameObject queueGo = new GameObject("BackgroundTaskQueue");
            BackgroundTaskQueue queue = queueGo.SetUpDisposeOnDestroy(BackgroundTaskQueue.NewBackgroundTaskQueue(1));

            var task1 = queue.Run(async (c) => { await TaskV2.Delay(10); });
            var task2 = queue.Run(async (c) => { await TaskV2.Delay(10); });

            queueGo.Destroy();

            yield return task1.AsCoroutine(onError: e => {
                Assert.True(e is AggregateException a1 && a1.InnerException is AggregateException a2 && a2.InnerException is TaskCanceledException);
            });
            Assert.IsTrue(task1.IsFaulted); // Task 1 never completed successfully since it was interrupted by the cancel
            Assert.IsTrue(task2.IsCanceled); // Task 2 was never started

            Log.MethodDone(timing);
        }

        [UnityTest]
        public IEnumerator TestRunTaskInBackground() {

            var queue = BackgroundTaskQueue.NewBackgroundTaskQueue(1);

            var task1WasStarted = false;
            var task1ReachedItsLoopEnd = false;
            var task1 = queue.Run(async (cancelRequest) => {
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
            });
            var errorWasThrown = false;
            yield return task1.AsCoroutine((e) => {
                Assert.IsTrue(e.GetBaseException() is MyException1, "e=" + e.GetBaseException().GetType());
                errorWasThrown = true;
            });
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(errorWasThrown);
            Assert.IsTrue(task1.IsFaulted);
            Assert.IsTrue(task1.Exception.GetBaseException() is MyException1);

            var task2 = queue.Run(async (cancelRequest) => {
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
            });
            yield return task2.AsCoroutine();
            Assert.IsTrue(task2.IsCompleted);
            Assert.IsFalse(task2.IsFaulted);
        }

        [UnityTest]
        public IEnumerator TestMainThread() {

            var queue = BackgroundTaskQueue.NewBackgroundTaskQueue(1);

            GameObject go = null;
            var task = queue.Run(async (cancel) => {
                cancel.ThrowIfCancellationRequested();
                // Test that its not be possible to create a GO in a background thread:
                AssertV2.Throws<Exception>(() => { go = new GameObject(name: "A"); });
                // Test that on MainThread the gameobject can be created:
                MainThread.Invoke(() => { go = new GameObject(name: "B"); });
                await TaskV2.Delay(1000); // wait for main thread action to execute
                Assert.IsTrue(go != null);
                // Assert.AreEqual("B", go.name); // go.name not allowed in background thread
                Log.d("Background thread now done");
            });
            Assert.IsNull(go);
            yield return task.AsCoroutine();
            task.ThrowIfException();
            Assert.IsNotNull(go);
            Assert.AreEqual("B", go.name);
        }

        [Serializable]
        private class MyException1 : Exception {
            public MyException1(string message) : base(message) { }
            // Required default constructors (https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1032):
            public MyException1() { }
            public MyException1(string message, Exception innerException) : base(message, innerException) { }
        }

    }

}
