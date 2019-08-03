using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.async {
    public class EventHandlerTests {

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
            await Task.Delay(50);
            Assert.Equal(2, counter);

            throttledAction(this, "good");
            throttledAction(this, "bad");
            throttledAction(this, "good");
            Assert.Equal(3, counter);
            await Task.Delay(100);
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
            await Task.Delay(100);
            Assert.Equal(2, counter);

        }

    }
}