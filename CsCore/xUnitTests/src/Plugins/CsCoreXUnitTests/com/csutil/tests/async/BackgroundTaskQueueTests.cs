using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class BackgroundTaskQueueTests {

        public BackgroundTaskQueueTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestRunWithTaskScheduler1() { // Aligned with the coroutine test TestExecuteRepeated1

            var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueue(maxConcurrencyLevel: 1);

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
        public async Task TestRunWithTaskScheduler2() { // Aligned with the coroutine test TestExecuteRepeated1

            var taskQueue = BackgroundTaskQueue.NewBackgroundTaskQueue(maxConcurrencyLevel: 2);

            // Create both tasks at the same time:
            Task t1 = taskQueue.Run(SomeAsyncTask1);
            Task<string> t2 = taskQueue.Run(SomeAsyncTask2);
            var t3 = taskQueue.Run(SomeAsyncTask1); // Add a 3rd task (will not be started)
            Assert.Equal(3, taskQueue.GetRemainingScheduledTaskCount());

            await t2;
            // Since the scheduler allows 2 tasks at a time, t1 will not be complete when t2 is done:
            Assert.False(t1.IsCompleted);
            Assert.Equal(1, taskQueue.GetCompletedTasksCount());

            await taskQueue.WhenAllTasksCompleted(flushQueueAfterCompletion: true);
            Assert.Equal(0, taskQueue.GetRemainingScheduledTaskCount());
            Assert.Equal(0, taskQueue.GetCompletedTasksCount()); // Queue was flushed

        }

        private async Task SomeAsyncTask1() {
            var t = Log.MethodEntered();
            await TaskV2.Delay(500);
            Log.MethodDone(t);
        }

        private async Task<string> SomeAsyncTask2() {
            var t = Log.MethodEntered();
            await TaskV2.Delay(5);
            Log.MethodDone(t);
            return "Some string result";
        }

    }

}