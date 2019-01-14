using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.csutil {

    class TestTasks {

        [SetUp]
        public void BeforeEachTest() { }

        [TearDown]
        public void AfterEachTest() { }

        [UnityTest]
        public IEnumerator TestRunTaskInBackground() {

            // since both tasks share the same scheduler task 2 will wait for task 1
            var scheduler = new QueuedTaskScheduler(1);

            var task1 = TaskRunner.instance.RunInBackground((cancelRequest) => {
                for (int i = 0; i < 5; i++) {
                    cancelRequest.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                    Log.d("Task 1: Step " + i);
                }
                Log.d("Task is done now");
                throw new Exception("Now the task will fault");
            }, scheduler).task;

            var task2 = TaskRunner.instance.RunInBackground((cancelRequest) => {
                for (int i = 0; i < 5; i++) {
                    cancelRequest.ThrowIfCancellationRequested();
                    Thread.Sleep(1000);
                    Log.d("Task 2: Step " + i);
                }
                Log.d("Task 2 is done now");
            }, scheduler).task;

            yield return task1.WaitForTaskToFinish();
            Assert.IsTrue(task1.IsCompleted);
            Assert.IsTrue(task1.IsFaulted);
            Log.d("Task 1 error was: " + task1.Exception);

            yield return task2.WaitForTaskToFinish();
            Assert.IsTrue(task2.IsCompleted);
            Assert.IsFalse(task2.IsFaulted);
        }

        [UnityTest]
        public IEnumerator TestMainThread() {
            GameObject go = null;
            var task = TaskRunner.instance.RunInBackground(delegate {
                // Test that its not be possible to create a GO in a background thread:
                AssertV2.Throws<Exception>(() => { go = new GameObject(name: "A"); });
                // Test that on MainThread the gameobject can be created:
                MainThread.Invoke(() => { go = new GameObject(name: "B"); });
                Thread.Sleep(1000); // wait for main thread action to execute
                Log.d("Background thread now done");
            }).task;
            Assert.IsNull(go);
            yield return task.WaitForTaskToFinish();
            task.ThrowIfException();
            Assert.IsNotNull(go);
            Assert.AreEqual("B", go.name);
        }


    }

}
