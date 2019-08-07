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
        public async void ThrottledDebounceTest1() {
            int counter = 0;
            EventHandler<string> action = (_, myStringParam) => {
                Assert.NotEqual("bad", myStringParam);
                Interlocked.Increment(ref counter);

            };
            var throttledAction = action.AsThrottledDebounce(delayInMs: 5);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            Assert.Equal(1, counter);
            for (int i = 0; i < 20; i++) { await Task.Delay(100); if (counter >= 2) { break; } }
            Assert.Equal(2, counter);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            Assert.Equal(3, counter);
            for (int i = 0; i < 20; i++) { await Task.Delay(100); if (counter >= 4) { break; } }
            Assert.Equal(4, counter);
        }

        [Fact]
        public async void ThrottledDebounceTest2() {
            int counter = 0;
            EventHandler<int> action = (_, myIntParam) => {
                Log.d("myIntParam=" + myIntParam);
                Interlocked.Increment(ref counter);
            };
            var throttledAction = action.AsThrottledDebounce(delayInMs: 5);

            var tasks = new List<Task>();
            for (int i = 0; i < 100; i++) { // Do 100 calls of the method in parallel:
                var myIntParam = i;
                tasks.Add(Task.Run(() => { throttledAction(this, myIntParam); }));
            }
            await Task.WhenAll(tasks.ToArray());
            await Task.Delay(1000);
            Assert.Equal(2, counter);
        }

        [Fact]
        public async void ExponentialBackoffExample1() {
            Stopwatch timer = Stopwatch.StartNew();
            var finalTimingResult = await TryWithExponentialBackoff<long>(async () => {
                await Task.Delay(5);
                Log.d("Task exec at " + timer.ElapsedMilliseconds);
                // In the first second of the test simulate errors:
                if (timer.ElapsedMilliseconds < 1000) { throw new TimeoutException("e.g. some network error"); }
                return timer.ElapsedMilliseconds;
            });
            Assert.True(1000 < finalTimingResult && finalTimingResult < 2000, "finalTimingResult=" + finalTimingResult);
        }

        [Fact]
        public async void ExponentialBackoffExample2() {
            Stopwatch timer = Stopwatch.StartNew();
            var result = await TryWithExponentialBackoff<long>(async () => {
                await Task.Delay(5);
                // In the first second of the test simulate errors:
                if (timer.ElapsedMilliseconds < 1000) { throw new TimeoutException("e.g. some network error"); }
                return timer.ElapsedMilliseconds;
            }, (e) => { // Errors could be logged or based on the type rethrown:
                Assert.IsType<TimeoutException>(e);
            });
            Assert.True(1000 < result && result < 2000, "result=" + result);
        }

        private static async Task<T> TryWithExponentialBackoff<T>(Func<Task<T>> getTask, Action<Exception> onError = null) {
            int maxNrOfRetries = -1;
            int maxDelayInMs = -1;
            int initialExponent = 1;
            int retryCount = initialExponent;
            Stopwatch timer = Stopwatch.StartNew();
            do {
                timer.Restart();
                try {
                    Task<T> task = getTask();
                    var result = await task;
                    if (task.IsCompletedSuccessfully) { return result; }
                } catch (Exception e) { onError.InvokeIfNotNull(e); }
                retryCount++;
                int delay = (int)(Math.Pow(2, retryCount) - timer.ElapsedMilliseconds);
                if (delay > maxDelayInMs && maxDelayInMs > 0) { delay = maxDelayInMs; }
                if (delay > 0) { await Task.Delay(delay); }
                if (retryCount > maxNrOfRetries && maxNrOfRetries > 0) {
                    throw new Exception("No success after " + retryCount + " retries");
                }
            } while (true);
        }

    }
}