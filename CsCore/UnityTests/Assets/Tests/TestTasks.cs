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
        public void BeforeEachTest() {
            UnitySetup.instance.Setup();
        }

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

    }

}
