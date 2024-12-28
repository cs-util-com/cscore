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

            await t2;
            // Since the scheduler allows 2 tasks at a time, t1 will not be complete when t2 is done:
            Assert.False(t1.IsCompleted);
            Assert.Equal(1, taskQueue.GetCompletedTasksCount());
            Assert.Equal(1, taskQueue.ProgressListener.GetCount()); // 1 Task should be completed

            await taskQueue.WhenAllTasksCompleted(flushQueueAfterCompletion: true);
            Assert.Equal(0, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(0, taskQueue.GetCompletedTasksCount()); // Queue was flushed
            Assert.Equal(100, taskQueue.ProgressListener.percent); // All tasks should be compleded

        }

        [Fact]
        public async Task TestCancelRequest() {

            using var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueueV2(maxConcurrencyLevel: 1);

            // Create both tasks at the same time:
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task<string> t2 = taskQueue.Run(SomeAsyncTask2);
            Task t3 = taskQueue.Run(SomeAsyncTask1); // Add a 3rd task (will not be started)

            taskQueue.CancelAllOpenTasks();
            // Awaiting the canceled queue will throw a TaskCanceledException:
            var exceptions = await Assert.ThrowsAsync<OperationCanceledException>(async () => {
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
                // Since t1 is already running when cancel is called an aggregate exception is created and t1 is set to faulted:
                await Assert.ThrowsAsync<OperationCanceledException>(() => Task.WhenAll(t1, t2, t3));

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
                // Since t2 is already running when cancel is called an aggregate exception is created and t2 is set to faulted:
                await Assert.ThrowsAsync<OperationCanceledException>(() => Task.WhenAll(t1, t2, t3));
                Assert.True(t1.IsCompletedSuccessfull()); // Only t2 and t3 are canceled by the custom token source
                Assert.True(t2.IsCanceled);
                Assert.True(t3.IsCanceled);
            }
            {
                var t = new CancellationTokenSource();
                Task t1 = taskQueue.Run(SomeAsyncTask1);
                Task<string> t2 = taskQueue.Run(SomeAsyncTask2, t);
                Task<string> t3 = taskQueue.Run(SomeAsyncTask2, t);

                t.Cancel();

                // Since t2 is not yet running when cancel is called both tasks are set to canceled:
                await Assert.ThrowsAsync<TaskCanceledException>(() => Task.WhenAll(t1, t2, t3));
                Assert.True(t1.IsCompletedSuccessfull()); // Only t2 and t3 are canceled by the custom token source
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

        private async Task SomeAsyncTask1(CancellationToken cancelRequest) {
            var t = Log.MethodEntered();
            await TaskV2.Delay(500);
            Log.MethodDone(t);
        }

        private async Task<string> SomeAsyncTask2(CancellationToken cancelRequest) {
            var t = Log.MethodEntered();
            await TaskV2.Delay(5);
            Log.MethodDone(t);
            return "Some string result";
        }

    }

}