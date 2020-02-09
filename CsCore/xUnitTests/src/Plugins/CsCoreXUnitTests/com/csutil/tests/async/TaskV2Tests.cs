using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {
    public class TaskV2Tests {

        public TaskV2Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            Log.d("Now testing TaskV2.Run");
            await TaskV2.Run(() => {
                var t = Log.MethodEntered("1");
                TaskV2.Delay(100).ContinueWith(delegate {
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
                TaskV2.RunRepeated(async () => {
                    counter++;
                    return counter < 3; // stop repeated execution once 3 is reached
                }, delayInMsBetweenIterations: 300, cancel.Token, delayInMsBeforeFirstExecution: 200);
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
        public async Task testNotAwaitingAsyncTask() {
            bool b1 = false, b2 = false;
            RunSomeAsyncTask(() => b1 = true);
            Assert.False(b1);
            await RunSomeAsyncTask(() => b2 = true);
            Assert.True(b2);
            Assert.True(b1);
        }

        private async Task RunSomeAsyncTask(Action p) {
            await TaskV2.Delay(50);
            p();
        }
    }
}