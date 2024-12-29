using com.csutil.progress;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.integrationTests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class BackgroundTaskQueueTests {

        public BackgroundTaskQueueTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 1);

            // Create both tasks at the same time:
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task<string> t2 = taskQueue.Run(SomeAsyncTask2);

            Assert.Equal(2, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(0, taskQueue.GetCompletedTasksCount());
            Assert.Equal(2, taskQueue.GetTotalTasksCount());
            await t1;

            Assert.Equal(1, taskQueue.GetRemainingScheduledTaskCount()); // 1 task started and 1 waiting
            Assert.Equal(1, taskQueue.GetCompletedTasksCount()); // 1 task started and 1 waiting
            Assert.Equal(2, taskQueue.GetTotalTasksCount());
            Assert.Equal("Some string result", await t2);

            Assert.Equal(0, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(2, taskQueue.GetCompletedTasksCount());
            Assert.Equal(2, taskQueue.GetTotalTasksCount());

            // Since the scheduler allows only one task at a time, if t2 is done, t1 also must be completed:
            Assert.True(t1.IsCompleted);

        }

        [Fact]
        public async Task TestConcurrency2() {

            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 2);
            taskQueue.ProgressListener = new ProgressV2("Progress Listener", 0);

            // Create both tasks at the same time:
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task<string> t2 = taskQueue.Run(SomeAsyncTask2);
            var t3 = taskQueue.Run(SomeAsyncTask1); // Add a 3rd task (will not be started)
            Assert.Equal(3, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(0, taskQueue.ProgressListener.percent);

            Assert.Equal(TaskStatus.WaitingForActivation, t1.Status);
            Assert.Equal(TaskStatus.WaitingForActivation, t2.Status);
            Assert.Equal(TaskStatus.WaitingForActivation, t3.Status);
            await t2;
            Assert.Equal(TaskStatus.RanToCompletion, t2.Status);
            Assert.Null(t1.Exception);
            Assert.Null(t3.Exception);
            Assert.NotEqual(TaskStatus.RanToCompletion, t3.Status);
            Assert.NotEqual(TaskStatus.RanToCompletion, t1.Status);
            // Since the scheduler allows 2 tasks at a time, t1 will not be complete when t2 is done:
            Assert.True(t2.IsCompletedSuccessfull());
            Assert.Equal(1, taskQueue.GetCompletedTasksCount());
            Assert.Equal(1, taskQueue.ProgressListener.GetCount()); // 1 Task should be completed
            Assert.False(t1.IsCompleted);

            await taskQueue.WhenAllTasksCompleted(flushQueueAfterCompletion: true);
            Assert.Equal(0, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(0, taskQueue.GetCompletedTasksCount()); // Queue was flushed
            Assert.Equal(100, taskQueue.ProgressListener.percent); // All tasks should be compleded

        }

        private async Task SomeAsyncTask1(CancellationToken cancelRequest) {
            var t = Log.MethodEntered();
            await Task.Delay(500, cancelRequest);
            Log.MethodDone(t);
        }

        private async Task<string> SomeAsyncTask2(CancellationToken cancelRequest) {
            var t = Log.MethodEntered();
            await Task.Delay(5, cancelRequest);
            Log.MethodDone(t);
            return "Some string result";
        }

        [Fact]
        public async Task TestCancelRequest() {

            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 1);

            // Create both tasks at the same time:
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task<string> t2 = taskQueue.Run(SomeAsyncTask2);
            Task t3 = taskQueue.Run(SomeAsyncTask1); // Add a 3rd task (will not be started)

            taskQueue.CancelAllOpenTasks();
            // Awaiting the canceled queue will throw OperationCanceledException:
            await Assert.ThrowsAsync<TaskCanceledException>(async () => {
                await taskQueue.WhenAllTasksCompleted();
            });


            // The first task was started but then canceled while it was already running:
            Assert.True(t1.IsCanceled);
            Assert.False(t1.IsCompletedSuccessfully);
            Assert.False(t1.IsFaulted);

            // The other 2 were canceled before they started:
            Assert.True(t2.IsCanceled);
            Assert.True(t3.IsCanceled);

        }

        [Fact]
        public async Task TestCancelRequestWithCustomTokenSource() {

            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 1);
            {
                var t = new CancellationTokenSource();
                Task t1 = taskQueue.Run(SomeAsyncTask1, t);
                Task t2 = taskQueue.Run(SomeAsyncTask1, t);
                Task<string> t3 = taskQueue.Run(SomeAsyncTask2);

                t.Cancel();
                // Since t1 is already running when cancel is called, we expect OperationCanceledException:
                await Assert.ThrowsAsync<TaskCanceledException>(() => Task.WhenAll(t1, t2, t3));

                Assert.True(t1.IsCanceled);
                Assert.True(t2.IsCanceled);
                Assert.True(t3.IsCompletedSuccessfull()); // Only t1 and t2 are canceled by the custom token source
            }
            {
                var t = new CancellationTokenSource();
                Task<string> t2 = taskQueue.Run(SomeAsyncTask2, t);
                Task<string> t3 = taskQueue.Run(SomeAsyncTask2, t);
                Task t1 = taskQueue.Run(SomeAsyncTask1);

                t.Cancel();
                await Assert.ThrowsAsync<TaskCanceledException>(() => Task.WhenAll(t1, t2, t3));
                Assert.True(t1.IsCompletedSuccessfull());
                Assert.True(t2.IsCanceled);
                Assert.True(t3.IsCanceled);
            }
            {
                var t = new CancellationTokenSource();
                Task t1 = taskQueue.Run(SomeAsyncTask1);
                Task<string> t2 = taskQueue.Run(SomeAsyncTask2, t);
                Task<string> t3 = taskQueue.Run(SomeAsyncTask2, t);

                t.Cancel();
                await Assert.ThrowsAsync<TaskCanceledException>(() => Task.WhenAll(t1, t2, t3));
                Assert.True(t1.IsCompletedSuccessfull());
                Assert.True(t2.IsCanceled);
                Assert.True(t3.IsCanceled);
            }
            {
                var t = new CancellationTokenSource();
                Task t1 = taskQueue.Run(SomeAsyncTask1);
                Task<string> t2 = taskQueue.Run(SomeAsyncTask2, t);
                Task<string> t3 = taskQueue.Run(SomeAsyncTask2, t);

                taskQueue.CancelAllOpenTasks(); // Canceling all tasks via the task queue is still possible

                var e = await Assert.ThrowsAsync<OperationCanceledException>(() => Task.WhenAll(t1, t2, t3));
                Assert.True(t1.IsCanceled);
                Assert.True(t2.IsCanceled);
                Assert.True(t3.IsCanceled);
            }
        }

        /// <summary>
        /// 1) Verifies the actual concurrency never exceeds the queue's max concurrency.
        /// 2) Demonstrates usage with multiple tasks to measure concurrency.
        /// </summary>
        [Fact]
        public async Task TestActualMaxConcurrencyNeverExceeded() {
            // We'll measure how many tasks are running at once and ensure it never goes above the limit
            const int maxConcurrencyLevel = 3;
            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel);
            int currentRunningTasks = 0;
            int maxObservedRunningTasks = 0;
            int totalTasks = 10;

            var tasks = Enumerable.Range(0, totalTasks).Select(async i => {
                await taskQueue.Run(async ct => {
                    // Increment the concurrency counter
                    int running = Interlocked.Increment(ref currentRunningTasks);
                    maxObservedRunningTasks = Math.Max(maxObservedRunningTasks, running);

                    // Simulate some work
                    await TaskV2.Delay(50);

                    // Decrement the concurrency counter
                    Interlocked.Decrement(ref currentRunningTasks);
                });
            });

            // Wait for all tasks to finish
            await Task.WhenAll(tasks);
            Assert.True(maxObservedRunningTasks <= maxConcurrencyLevel,
                $"Observed concurrency {maxObservedRunningTasks} exceeded the limit of {maxConcurrencyLevel}");
        }

        /// <summary>
        /// Test that tasks which throw an exception are faulted, 
        /// and the exception is propagated via WhenAllTasksCompleted.
        /// </summary>
        [Fact]
        public async Task TestExceptionPropagation() {
            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 2);

            var taskOk = taskQueue.Run(SomeAsyncTask1); // This one should succeed
            var taskFail = taskQueue.Run(async ct => {
                await TaskV2.Delay(10);
                throw new InvalidOperationException("Some custom failure");
            });

            // When waiting for tasks to complete, we should see the exception from taskFail
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => taskQueue.WhenAllTasksCompleted());
            Assert.Equal("Some custom failure", ex.Message);

            Assert.True(taskOk.IsCompletedSuccessfully);
            Assert.True(taskFail.IsFaulted);
            Assert.IsType<InvalidOperationException>(taskFail.Exception?.GetBaseException());
        }

        /// <summary>
        /// Test what happens if the queue is disposed while tasks are still running.
        /// Behavior may depend on your desired logic:
        /// - If you want to allow tasks to complete, the queue disposal might only prevent new tasks from starting.
        /// - Or you may want to trigger a cancellation on disposal.
        /// </summary>
        [Fact]
        public async Task TestDisposalWhileTasksAreRunning() {
            var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 2);
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task t2 = taskQueue.Run(SomeAsyncTask2);
            // Immediately dispose the queue before tasks finish
            taskQueue.Dispose();
            try {
                await Task.WhenAll(t1, t2);
            } catch (ObjectDisposedException) {
                // Happens because Dispose() calls .CancelAllOpenTasks()
            }
            Assert.True(t1.IsFaulted);
            Assert.True(t2.IsFaulted);
        }

        [Fact]
        public async Task TestDisposalWhileTasksAreRunning2() {
            var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 2);
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task t2 = taskQueue.Run(SomeAsyncTask2);
            // Immediately cancel all tasks before they finish
            taskQueue.CancelAllOpenTasks();
            try {
                await Task.WhenAll(t1, t2);
            } catch (OperationCanceledException) {
                // Happens because CancelAllOpenTasks() was called
            }
            Assert.True(t1.IsCanceled);
            Assert.True(t2.IsCanceled);
        }


    }

}