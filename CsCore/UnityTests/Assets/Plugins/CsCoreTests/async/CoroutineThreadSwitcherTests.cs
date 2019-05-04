using System;
using UnityEngine;
using System.Collections;
using System.Threading;
using UnityEngine.TestTools;
using NUnit.Framework;

namespace com.csutil.tests.async {

    public class CoroutineThreadSwitcherTests {

        [UnityTest]
        public IEnumerator TestRunTaskInBackground() {


            var go = new GameObject();
            MonoBehaviour x = go.GetOrAddComponent<MyExampleMono1>();
            Assert.IsNotNull(x);

            x.StartCoroutineInBgThread(MyAsyncTask());
            yield return x.StartCoroutine(OtherExampleTasks(x));

        }

        IEnumerator MyAsyncTask() {
            Assert.IsFalse(MainThread.isMainThread);
            Thread.Sleep(3000); // Won't block the main thread

            yield return ThreadSwitcher.ToMainThread;
            Assert.IsTrue(MainThread.isMainThread);

            yield return ThreadSwitcher.ToBackgroundThread;
            Assert.IsFalse(MainThread.isMainThread);
        }

        IEnumerator OtherExampleTasks(MonoBehaviour x) {
            AsyncTask task;
            Assert.IsTrue(MainThread.isMainThread);
            x.StartCoroutineInBgThread(BlockingTask(), out task);
            yield return x.StartCoroutine(task.Wait());
            Assert.AreEqual(TaskState.Done, task.State);

            x.StartCoroutineInBgThread(TaskWhichWillBeCancelled(), out task);
            yield return new WaitForSeconds(2.0f);
            task.Cancel();
            Assert.AreEqual(TaskState.Cancelled, task.State);

        }

        IEnumerator BlockingTask() {
            Assert.IsFalse(MainThread.isMainThread);
            Thread.Sleep(2000);

            yield return ThreadSwitcher.ToMainThread;
            yield return new WaitForSeconds(0.1f);
            Thread.Sleep(2000); // will block unity main thread

            yield return ThreadSwitcher.ToBackgroundThread;
            yield return new WaitForSeconds(2.0f); // WaitForSeconds on background
        }

        IEnumerator TaskWhichWillBeCancelled() {
            Assert.IsFalse(MainThread.isMainThread);
            for (int i = 0; i < int.MaxValue; i++) { Thread.Sleep(50000); }
            yield break;
        }

    }

}