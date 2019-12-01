using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {

    public class EventHandlerTests {

        public EventHandlerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExponentialBackoffExample1() {
            Stopwatch timer = Stopwatch.StartNew();
            var finalTimingResult = await TaskV2.TryWithExponentialBackoff<long>(async () => {
                await TaskV2.Delay(5);
                Log.d("Task exec at " + timer.ElapsedMilliseconds);
                // In the first second of the test simulate errors:
                if (timer.ElapsedMilliseconds < 1000) { throw new TimeoutException("e.g. some network error"); }
                return timer.ElapsedMilliseconds;
            });
            Assert.True(1000 < finalTimingResult && finalTimingResult < 3000, "finalTimingResult=" + finalTimingResult);
        }

        [Fact]
        public async Task ExponentialBackoffExample2() {
            Stopwatch timer = Stopwatch.StartNew();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => {

                // Try 5 times to execute to run someTaskThatFailsEveryTime:
                await TaskV2.TryWithExponentialBackoff(SomeTaskThatFailsEveryTime, (e) => {
                    Assert.IsType<TimeoutException>(e); // Errors could be logged or based on the type rethrown
                }, maxNrOfRetries: 5, maxDelayInMs: 200); // The exponential backoff will not be larger then 200ms

            });
            var finalTimingResult = timer.ElapsedMilliseconds;
            Assert.True(finalTimingResult < 6000, "finalTimingResult=" + finalTimingResult);
        }

        private async Task SomeTaskThatFailsEveryTime() {
            await TaskV2.Delay(5);
            throw new TimeoutException("e.g. some network error");
        }

        [Fact]
        public async Task TestTaskV2Delay() {
            var t1 = Stopwatch.StartNew();
            var t2 = StopwatchV2.StartNewV2("TestTaskV2Delay");
            var min = 0;
            var delayInMs = 100;
            for (int i = 0; i < 5; i++) {
                min += delayInMs;
                await TaskV2.Delay(delayInMs);
                Assert.InRange(t1.ElapsedMilliseconds, min * 0.05f, (min + delayInMs) * 5f);
                Assert.InRange(t2.ElapsedMilliseconds, min * 0.05f, (min + delayInMs) * 5f);
            }
        }

        [Fact]
        public async Task TestThrottledDebounce1() {
            int counter = 0;
            EventHandler<string> action = (_, myStringParam) => {
                Assert.NotEqual("bad", myStringParam);
                Log.d("action callback with old counter=" + counter);
                Interlocked.Increment(ref counter);
                Log.d("... new counter=" + counter);
            };
            var throttledAction = action.AsThrottledDebounce(delayInMs: 5);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            for (int i = 0; i < 30; i++) { await TaskV2.Delay(100); if (counter >= 2) { break; } }
            Assert.Equal(2, counter);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            for (int i = 0; i < 30; i++) { await TaskV2.Delay(100); if (counter >= 4) { break; } }
            await TaskV2.Delay(100);
            Assert.Equal(4, counter);
            await TaskV2.Delay(100);
            Assert.Equal(4, counter);

        }

        [Fact]
        public async Task TestThrottledDebounce2() {
            int counter = 0;
            EventHandler<int> action = (_, myIntParam) => {
                Log.d("myIntParam=" + myIntParam);
                Interlocked.Increment(ref counter);
            };
            var throttledAction = action.AsThrottledDebounce(delayInMs: 5);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++) { // Do 100 calls of the method in parallel:
                var myIntParam = i;
                tasks.Add(TaskV2.Run(() => { throttledAction(this, myIntParam); }));
            }
            await Task.WhenAll(tasks.ToArray());
            for (int i = 0; i < 20; i++) { await TaskV2.Delay(5); if (counter >= 2) { break; } }
            Assert.Equal(2, counter);
            await TaskV2.Delay(100);
            Assert.Equal(2, counter);
        }

        [Fact]
        public async Task TestTaskV2() {

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

    }

}