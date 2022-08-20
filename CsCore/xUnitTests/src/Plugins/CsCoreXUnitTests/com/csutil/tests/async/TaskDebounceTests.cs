using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class TaskDebounceTests {

        public TaskDebounceTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestThrottledDebounce1() {

            int counter = 0;
            Func<string, Task> originalFunction = async (string param) => {
                if (param != "good") { throw new DataMisalignedException("param was " + param); }
                counter++;
                Log.MethodEnteredWith(param, counter);
            };
            int delayInMs = 500;
            var wrappedFunc = originalFunction.AsThrottledDebounce(delayInMs);
            {
                var t = wrappedFunc("good");
                Assert.Equal(1, counter); // Instant trigger
                await t;
                Assert.Equal(1, counter);
            }
            {

                var badTasks = new List<Task>();
                badTasks.Add(wrappedFunc("bad"));
                badTasks.Add(wrappedFunc("bad"));
                badTasks.Add(wrappedFunc("bad"));
                var t = wrappedFunc("good");
                Assert.Equal(1, counter); // Only triggered after delay
                await TaskV2.Delay(delayInMs * 3);
                Assert.Equal(2, counter);
                await t;
                Assert.Equal(2, counter);
                foreach (var bad in badTasks) {
                    Assert.True(bad.IsCompleted);
                    Assert.True(bad.IsCanceled);
                    Assert.False(bad.IsFaulted);
                }
            }

            {
                wrappedFunc("bad");
                wrappedFunc("bad");
                var t = wrappedFunc("good");
                await TaskV2.Delay(delayInMs * 3);
                await t;
                Assert.Equal(3, counter);
            }

        }

        [Fact]
        public async Task TestThrottledDebounce2() {
            int counter = 0;
            int delayInMs = 200;
            Func<object, Task> originalFunction = async (object param) => {
                Assert.Equal("good", param);
                counter++;
                await TaskV2.Delay(delayInMs * 4);
            };
            var wrappedFunc = originalFunction.AsThrottledDebounce(10, skipFirstEvent: true);
            {
                var t = Stopwatch.StartNew();
                var task = wrappedFunc("good");
                Assert.Equal(0, counter);
                await task;
                Assert.Equal(1, counter);
                Assert.True(t.ElapsedMilliseconds > delayInMs * 2, "ElapsedMilliseconds=" + t.ElapsedMilliseconds);
            }
            {
                var b = wrappedFunc("bad");
                wrappedFunc("bad");
                var t = wrappedFunc("good");
                Assert.Equal(1, counter);
                await TaskV2.Delay(delayInMs);
                Assert.Equal(2, counter);
                Assert.False(t.IsCompleted);
                await t;
                Assert.True(b.IsCompleted);
                Assert.False(b.IsCompletedSuccessfull());
                Assert.False(b.IsCompletedSuccessfully);
            }
        }

        [Fact]
        public async Task TestThrottledDebounce3() {
            int counter = 0;
            Func<Task> originalFunction = () => {
                counter++;
                throw new NotImplementedException();
            };
            var wrappedFunc = originalFunction.AsThrottledDebounce(10);

            {
                Assert.Equal(0, counter);
                var t = wrappedFunc();
                Assert.Equal(1, counter);
                Assert.True(t.IsFaulted);
                await Assert.ThrowsAsync<NotImplementedException>(() => t);
            }

            {
                Assert.Equal(1, counter);
                wrappedFunc();
                var t = wrappedFunc();
                Assert.Equal(1, counter);
                Assert.False(t.IsFaulted);
                await TaskV2.Delay(50);
                Assert.Equal(2, counter);
                Assert.True(t.IsFaulted);
                await Assert.ThrowsAsync<NotImplementedException>(() => t);
            }

        }

        [Fact]
        public async Task TestThrottledDebounce4() {

            int counter = 0;
            Func<string, Task<bool>> originalFunction = async (string param) => {
                if (param != "good") { throw new DataMisalignedException("param was " + param); }
                counter++;
                Log.MethodEnteredWith(param, counter);
                return true;
            };
            int delayInMs = 500;
            var wrappedFunc = originalFunction.AsThrottledDebounceV2(delayInMs);
            {
                var t = wrappedFunc("good");
                Assert.Equal(1, counter); // Instant trigger
                await t;
                Assert.Equal(1, counter);
            }
            {

                var badTasks = new List<Task>();
                badTasks.Add(wrappedFunc("bad"));
                badTasks.Add(wrappedFunc("bad"));
                badTasks.Add(wrappedFunc("bad"));
                var t = wrappedFunc("good");
                Assert.Equal(1, counter); // Only triggered after delay
                await TaskV2.Delay(delayInMs * 3);
                Assert.Equal(2, counter);
                await t;
                Assert.Equal(2, counter);
                foreach (var bad in badTasks) {
                    Assert.True(bad.IsCompleted);
                    Assert.True(bad.IsCanceled);
                    Assert.False(bad.IsFaulted);
                }
            }

            {
                wrappedFunc("bad");
                wrappedFunc("bad");
                var t = wrappedFunc("good");
                await TaskV2.Delay(delayInMs * 3);
                await t;
                Assert.Equal(3, counter);
            }

        }

        [Fact]
        public async Task TestThrottledDebounce5() {
            int counter = 0;
            int delayInMs = 200;
            Func<object, Task<bool>> originalFunction = async (object param) => {
                Assert.Equal("good", param);
                counter++;
                await TaskV2.Delay(delayInMs * 4);
                return false;
            };
            var wrappedFunc = originalFunction.AsThrottledDebounceV2(10, skipFirstEvent: true);
            {
                var t = Stopwatch.StartNew();
                var task = wrappedFunc("good");
                Assert.Equal(0, counter);
                await task;
                Assert.Equal(1, counter);
                Assert.True(t.ElapsedMilliseconds > delayInMs * 2, "ElapsedMilliseconds=" + t.ElapsedMilliseconds);
            }
            {
                var b = wrappedFunc("bad");
                wrappedFunc("bad");
                var t = wrappedFunc("good");
                Assert.Equal(1, counter);
                await TaskV2.Delay(delayInMs);
                Assert.Equal(2, counter);
                Assert.False(t.IsCompleted);
                await t;
                Assert.True(b.IsCompleted);
                Assert.False(b.IsCompletedSuccessfull());
                Assert.False(b.IsCompletedSuccessfully);
            }
        }

        [Fact]
        public async Task TestThrottledDebounce6() {
            int counter = 0;
            Func<Task> originalFunction = () => {
                counter++;
                throw new NotImplementedException();
            };
            var wrappedFunc = originalFunction.AsThrottledDebounceV2(10);

            {
                Assert.Equal(0, counter);
                var t = wrappedFunc();
                Assert.Equal(1, counter);
                Assert.True(t.IsFaulted);
                await Assert.ThrowsAsync<NotImplementedException>(() => t);
            }

            {
                Assert.Equal(1, counter);
                wrappedFunc();
                var t = wrappedFunc();
                Assert.Equal(1, counter);
                Assert.False(t.IsFaulted);
                await TaskV2.Delay(50);
                Assert.Equal(2, counter);
                Assert.True(t.IsFaulted);
                await Assert.ThrowsAsync<NotImplementedException>(() => t);
            }

        }

        [Fact]
        public async Task TestThrottledDebounce7() {
            bool entered = false;
            Func<int, Task> t = async (millisecondsDelay) => {
                Assert.False(entered);
                entered = true;
                await TaskV2.Delay(millisecondsDelay);
                entered = false;
            };
            var tDebounced = t.AsThrottledDebounceV2(200);

            var ms = 100;
            var t1 = tDebounced(ms);
            var t2 = tDebounced(ms);
            var t3 = tDebounced(ms);
            var t4 = tDebounced(ms);

            Assert.True(await t1);
            Assert.False(await t2);
            Assert.False(await t3);
            Assert.True(await t4);
        }

        [Fact]
        public async Task TestThrottledDebounce8() {

            bool entered = false;
            Func<Task> t = async () => {
                Assert.False(entered);
                entered = true;
                await TaskV2.Delay(200);
                entered = false;
            };
            var tDebounced = t.AsThrottledDebounceV2(100);

            var t1 = tDebounced();
            var t2 = tDebounced();
            var t3 = tDebounced();
            var t4 = tDebounced();

            Assert.True(await t1);
            Assert.False(await t2);
            Assert.False(await t3);
            Assert.True(await t4);

        }

        [Fact]
        public async Task TestThrottledDebounce9() {

            bool entered = false;
            Func<Task> t = async () => {
                Assert.False(entered);
                entered = true;
                await TaskV2.Delay(100);
                entered = false;
            };
            var tDebounced = t.AsThrottledDebounceV2(200);

            var t1 = tDebounced();
            var t2 = tDebounced();
            var t3 = tDebounced();
            var t4 = tDebounced();

            Assert.True(await t1);
            Assert.False(await t2);
            Assert.False(await t3);
            Assert.True(await t4);

        }

    }
}