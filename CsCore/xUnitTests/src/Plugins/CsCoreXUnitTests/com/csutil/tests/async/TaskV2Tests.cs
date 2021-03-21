using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using Xunit;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class TaskV2Tests {

        public TaskV2Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            Log.d("Now testing TaskV2.Run");
            await TaskV2.Run(() => {
                var t = Log.MethodEntered("1");
                TaskV2.Delay(100).ContinueWithSameContext(delegate {
                    Log.MethodDone(t);
                });
            });

            Log.d("Now testing async TaskV2.Run");
            await TaskV2.Run(async () => {
                var t = Log.MethodEntered("2");
                await TaskV2.Delay(100);
                Log.MethodDone(t);
            });

            Log.d("Now testing async TaskV2.Run with return value");
            var result = await TaskV2.Run(async () => {
                var t = Log.MethodEntered("3");
                await TaskV2.Delay(100);
                Log.MethodDone(t);
                return "3";
            });
            Assert.Equal("3", result);

        }

        [Fact]
        public async Task TestRunRepeated1() { // Aligned with the coroutine test TestExecuteRepeated1

            var counter = 0;
            var cancel = new CancellationTokenSource();
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                TaskV2.RunRepeated(async () => {
                    await TaskV2.Delay(1);
                    counter++;
                    return counter < 3; // stop repeated execution once 3 is reached
                }, delayInMsBetweenIterations: 300, cancelToken: cancel.Token, delayInMsBeforeFirstExecution: 200);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            { // while the repeated task is running check over time if the counter increases:
                await TaskV2.Delay(100);
                Assert.Equal(0, counter);
                await TaskV2.Delay(200);
                Assert.Equal(1, counter);
                await TaskV2.Delay(300);
                Assert.Equal(2, counter);
                await TaskV2.Delay(300);
                Assert.Equal(3, counter);
                await TaskV2.Delay(300);
                Assert.Equal(3, counter);
            }
            cancel.Cancel();

        }

        [Fact]
        public async Task TestNotAwaitingAsyncTask() {
            bool b1 = false, b2 = false;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            var b1Task = RunSomeAsyncTask(() => b1 = true);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Assert.False(b1);
            await RunSomeAsyncTask(() => b2 = true);
            Assert.True(b2);
            await b1Task;
            Assert.True(b1);
        }

        private async Task RunSomeAsyncTask(Action p) {
            await TaskV2.Delay(50);
            p();
        }

        [Fact]
        public async Task TestRunWithTaskScheduler1() { // Aligned with the coroutine test TestExecuteRepeated1

            QueuedTaskScheduler scheduler = new QueuedTaskScheduler(TaskScheduler.Default, maxConcurrencyLevel: 1);
            var cancel = new CancellationTokenSource();

            // Create both tasks at the same time:
            Task t1 = TaskV2.Run(SomeAsyncTask1, cancel, scheduler);
            Task<string> t2 = TaskV2.Run(SomeAsyncTask2, cancel, scheduler);

            Assert.Equal(1, scheduler.GetRemainingScheduledTaskCount()); // 1 task started and 1 waiting
            Assert.Equal("Some string result", await t2);
            Assert.Equal(0, scheduler.GetRemainingScheduledTaskCount());

            // Since the scheduler allows only one task at a time, if t2 is done, t1 also must be completed:
            Assert.True(t1.IsCompleted);

        }

        [Fact]
        public async Task TestRunWithTaskScheduler2() { // Aligned with the coroutine test TestExecuteRepeated1

            var maxConcurrencyLevel = 2;
            QueuedTaskScheduler scheduler = new QueuedTaskScheduler(TaskScheduler.Default, maxConcurrencyLevel);
            var cancel = new CancellationTokenSource();

            // Create both tasks at the same time:
            Task t1 = TaskV2.Run(SomeAsyncTask1, cancel, scheduler);
            Task<string> t2 = TaskV2.Run(SomeAsyncTask2, cancel, scheduler);
            var t3 = TaskV2.Run(SomeAsyncTask1, cancel, scheduler); // Add a 3rd task (will not be started)
            Assert.True(scheduler.GetRemainingScheduledTaskCount() >= 1);

            Assert.Equal("Some string result", await t2);

            // Check that now also task t3 was started:
            Assert.Equal(0, scheduler.GetRemainingScheduledTaskCount());

            // Since the scheduler allows 2 tasks at a time, t1 will not be complete when t2 is done:
            Assert.False(t1.IsCompleted);
            await t1;
        }

        [Fact]
        public async Task TestOnError() {
            {
                var errorHandled = false;
                await SomeAsyncFailingTask1().OnError(async _ => {
                    await TaskV2.Delay(5);
                    errorHandled = true; // error can be rethrown here
                });
                Assert.True(errorHandled);
            }
            {
                var errorHandled = false;
                var result = await SomeAsyncFailingTask2().OnError(async _ => {
                    await TaskV2.Delay(5);
                    errorHandled = true;
                    return "handled";
                });
                Assert.Equal("handled", result);
                Assert.True(errorHandled);
            }
            { // The error can be rethrown in OnError handler after reacting to it:
                await Assert.ThrowsAsync<AggregateException>(async () => {
                    await SomeAsyncFailingTask1().OnError(async error => {
                        await TaskV2.Delay(5);
                        throw error;
                    });
                });
            }
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

        private async Task SomeAsyncFailingTask1() {
            var t = Log.MethodEntered();
            await TaskV2.Delay(5);
            throw new Exception("task failed as requested");
        }

        private async Task<string> SomeAsyncFailingTask2() {
            var t = Log.MethodEntered();
            await TaskV2.Delay(5);
            throw new Exception("task failed as requested");
        }

    }

}