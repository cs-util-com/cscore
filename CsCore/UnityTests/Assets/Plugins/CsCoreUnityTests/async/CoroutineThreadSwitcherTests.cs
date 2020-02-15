using System;
using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Threading.Tasks;

namespace com.csutil.tests.async {

    public class CoroutineThreadSwitcherTests {

        [UnityTest]
        public IEnumerator TestMixingBackgroundAndMainThread() {
            var timing = Log.MethodEntered("TestMixingBackgroundAndMainThread");

            var backgroundTask1IsDone = false;
            yield return TaskRunner.instance.RunInBackground(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                await TaskV2.Delay(100);
                backgroundTask1IsDone = true;
            }).AsCoroutine();

            Assert.IsTrue(backgroundTask1IsDone);
            Assert.IsTrue(MainThread.isMainThread);

            var backgroundTask2 = TaskRunner.instance.RunInBackground(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                await TaskV2.Delay(100);
                c.ThrowIfCancellationRequested();
                return "some result";
            });
            yield return backgroundTask2.AsCoroutine();
            Assert.IsTrue(backgroundTask2.task.IsCompleted);
            Assert.AreEqual("some result", backgroundTask2.task.Result);

            Assert.IsTrue(timing.ElapsedMilliseconds > 200, "t=" + timing.ElapsedMilliseconds);
            Log.MethodDone(timing);
        }

        [UnityTest]
        public IEnumerator TestTaskCancel() {
            var timing = Log.MethodEntered("TestTaskCancel");

            var counter = 0;
            var task1 = TaskRunner.instance.RunInBackground(async (c) => {
                Assert.IsFalse(MainThread.isMainThread);
                for (int i = 0; i < int.MaxValue; i++) {
                    await TaskV2.Delay(10);
                    counter++;
                    c.ThrowIfCancellationRequested();
                }
            });
            yield return new WaitForSeconds(0.5f);
            task1.cancelTask();
            yield return task1.AsCoroutine();
            Assert.IsTrue(task1.task.IsCanceled);
            Assert.IsTrue(counter > 10, "counter=" + counter);
            Assert.IsTrue(counter < 20, "counter=" + counter);

            Log.MethodDone(timing);
        }

    }

}