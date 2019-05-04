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
            var myMono = new GameObject().GetOrAddComponent<MyExampleMono1>();

            myMono.StartCoroutineInBgThread(MyBackgroundCoroutine1());
            yield return myMono.StartCoroutine(MyNormalCoroutine1(myMono));
        }

        IEnumerator MyBackgroundCoroutine1() {
            var t = Log.MethodEntered();

            Assert.IsFalse(MainThread.isMainThread);
            Thread.Sleep(2000); // Won't block the main thread

            yield return ThreadSwitcher.ToMainThread;
            Assert.IsTrue(MainThread.isMainThread);

            yield return ThreadSwitcher.ToBackgroundThread;
            Assert.IsFalse(MainThread.isMainThread);
            yield return new WaitForSeconds(1.0f); // WaitForSeconds on background

            Assert.IsTrue(t.ElapsedMilliseconds > 3000, "t=" + t.ElapsedMilliseconds);
        }

        IEnumerator MyNormalCoroutine1(MonoBehaviour x) {
            Log.MethodEntered();
            AsyncTask task;
            x.StartCoroutineInBgThread(MyBackgroundCoroutine1(), out task);
            yield return x.StartCoroutine(task.Wait());
            Assert.AreEqual(TaskState.Done, task.State);

            x.StartCoroutineInBgThread(MyNeverEndingBackgroundCoroutine1(), out task);
            yield return new WaitForSeconds(1.0f);
            task.Cancel();
            Assert.AreEqual(TaskState.Cancelled, task.State);
        }

        IEnumerator MyNeverEndingBackgroundCoroutine1() {
            Log.MethodEntered();
            Assert.IsFalse(MainThread.isMainThread);
            for (int i = 0; i < int.MaxValue; i++) { Thread.Sleep(10000); Log.d("TaskWhichWillBeCancelled Loop"); }
            yield break;
        }

    }

}