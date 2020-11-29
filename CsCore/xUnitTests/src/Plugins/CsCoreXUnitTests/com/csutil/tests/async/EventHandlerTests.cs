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
        public async Task ThrottledDebounceExample1() {
            int counter = 0;
            bool allWereGood = true;
            Action<string> action = (myStringParam) => {
                // Make sure the action is never called with "bad" being passed:
                if (myStringParam != "good") { allWereGood = false; }
                Interlocked.Increment(ref counter);
            };
            // Make the action throttled / debounced:
            action = action.AsThrottledDebounce(delayInMs: 50);

            // Call it multiple times with less then 50ms between the calls:
            action("good"); // The first call will always be passed through
            action("bad"); // This one will be delayed and not called because of the next:
            action("good"); // This will be delayed for 50ms and then triggered because no additional call follows after it

            // Wait a little bit until the action was triggered at least 2 times:
            for (int i = 0; i < 50; i++) { await TaskV2.Delay(100); if (counter >= 2) { break; } }
            Assert.Equal(2, counter);
            Assert.True(allWereGood);
        }

        [Fact]
        public async Task ExponentialBackoffExample1() {
            Stopwatch timer = Stopwatch.StartNew();
            long finalTimingResult = await TaskV2.TryWithExponentialBackoff<long>(async () => {
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
                Log.d("action callback with old counter=" + counter);
                Interlocked.Increment(ref counter);
                Log.d("... new counter=" + counter);
                Assert.NotEqual("bad", myStringParam);
            };
            var throttledAction = action.AsThrottledDebounce(delayInMs: 50);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            for (int i = 0; i < 30; i++) { await TaskV2.Delay(100); if (counter >= 2) { break; } }
            Assert.Equal(2, counter);
            await TaskV2.Delay(100);
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
        public async Task TestThrottledDebounce3() {
            int counter = 0;
            bool myStringParamWasBad = false;
            EventHandler<string> action = (_, myStringParam) => {
                Log.d("action callback with old counter=" + counter);
                Interlocked.Increment(ref counter);
                Log.d("... new counter=" + counter);
                if (myStringParam == "bad") { myStringParamWasBad = true; }
            };
            var t = 250;
            var throttledAction = action.AsThrottledDebounce(delayInMs: t * 2, skipFirstEvent: true);
            throttledAction(this, "bad");
            await TaskV2.Delay(t);
            Assert.Equal(0, counter);
            Assert.False(myStringParamWasBad);
            throttledAction(this, "bad");
            await TaskV2.Delay(t);
            Assert.Equal(0, counter);
            Assert.False(myStringParamWasBad);
            throttledAction(this, "bad");
            await TaskV2.Delay(t);
            Assert.Equal(0, counter);
            Assert.False(myStringParamWasBad);
            throttledAction(this, "bad");
            await TaskV2.Delay(t);
            Assert.Equal(0, counter);
            Assert.False(myStringParamWasBad);
            throttledAction(this, "good");
            await TaskV2.Delay(t * 4);
            Assert.Equal(1, counter);
            Assert.False(myStringParamWasBad);
        }

        [Fact]
        public async Task TestThrottledDebounce4() {
            Func<object, string> originalFunction = (object sender) => {
                Assert.Equal("good", sender);
                Log.d("sender=" + sender);
                return "awesome";
            };
            int delayInMs = 500;
            var wrappedFunc = originalFunction.AsThrottledDebounce(delayInMs);
            Assert.Equal("awesome", wrappedFunc("good"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            await TaskV2.Delay(delayInMs * 3);
            Assert.Equal("awesome", wrappedFunc("good"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
        }

        [Fact]
        public async Task TestThrottledDebounce5() {
            int delayInMs = 200;
            Func<object, Task> originalFunction = async (object sender) => {
                Assert.Equal("good", sender);
                await TaskV2.Delay(delayInMs * 4);
            };
            var wrappedFunc = originalFunction.AsThrottledDebounce(10);

            var t = Stopwatch.StartNew();
            var task = wrappedFunc("good");
            await TaskV2.Delay(delayInMs);
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            await task;
            task = wrappedFunc("good");
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            Assert.Null(wrappedFunc("bad"));
            await task;

            Assert.True(t.ElapsedMilliseconds > 200, "ElapsedMilliseconds=" + t.ElapsedMilliseconds);
        }

        [Fact]
        public async Task TestThrottledDebounce6() {
            bool wasCalled = false;
            Func<object, Task> originalFunction = (object sender) => {
                wasCalled = true;
                throw new NotImplementedException();
            };
            var wrappedFunc = originalFunction.AsThrottledDebounce(10);
            Assert.NotNull(wrappedFunc);
            Assert.False(wasCalled);
            Assert.Null(wrappedFunc("good"));
            Assert.True(wasCalled);
            await TaskV2.Delay(50); // To allow the error show in the log
        }

    }

}