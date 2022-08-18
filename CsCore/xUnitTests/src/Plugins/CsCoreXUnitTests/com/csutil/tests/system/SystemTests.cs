using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.system {

    public class SystemTests {

        public SystemTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestGuidV2() {

            for (int i = 0; i < 1000; i++) {
                var a = GuidV2.NewGuid();
                var b = GuidV2.NewGuid();
                //Assert.True(b.CompareTo(a) > 0, $"Run {i}: a={a} was > then b={b}");
                Assert.True(b.CompareToV2(a) > 0, $"Run {i}: a={a} was > then b={b}");
                Assert.True(a.CompareToV2(b) < 0, $"Run {i}: a={a} was > then b={b}");
                Assert.True(a.CompareToV2(a) == 0, $"Run {i}: a != a : {b}");
            }
            var count = 1000000;
            var t1 = StopwatchV2.StartNewV2();
            {
                var isOrderedAfterCounter = RunIdTest(count, () => Guid.NewGuid());
                // Guid.NewGuid are not ordered:
                Assert.NotEqual(0, isOrderedAfterCounter);
                Assert.NotEqual(count, isOrderedAfterCounter);
            }
            t1.StopV2();
            var t2 = StopwatchV2.StartNewV2();
            {
                var isOrderedAfterCounter = RunIdTest(count, () => GuidV2.NewGuid());
                // GuidV2.NewGuid must be ordered:
                Assert.Equal(count, isOrderedAfterCounter);
            }
            t2.StopV2();
            // Check that the GuidV2.NewGuid is not much slower then Guid.NewGuid
            Assert.True(t1.ElapsedMilliseconds * 2 > t2.ElapsedMilliseconds);
        }

        private static int RunIdTest(int count, Func<Guid> newId) {
            HashSet<Guid> all = new HashSet<Guid>();
            var isOrderedAfterCounter = 0;
            var lastId = newId();
            for (int i = 0; i < count; i++) {
                var id = newId();
                Assert.True(all.Add(id));
                bool isOrderedAfter = id.CompareToV2(lastId) > 0;
                if (isOrderedAfter) { isOrderedAfterCounter++; }
            }
            return isOrderedAfterCounter;
        }

        private class MyDisposable : IDisposable {
            public bool DEBUG_exceptionWasDetected = false;
            public void Dispose() {
                #if DEBUG // If true an exception was detected in the current context:
                DEBUG_exceptionWasDetected = this.DEBUG_ThrownExceptionDetectedInCurrentContext();
                #endif
            }
        }

        [Fact]
        public async Task TestIDisposableWithException() {
            { // If dispose is called because of the using block being complete the flag has to be false:
                var d = new MyDisposable();
                using (d) { }
                Assert.False(d.DEBUG_exceptionWasDetected);
            }
            { // If dispose is called because of an exception the flag has to be true:
                var d = new MyDisposable();
                try {
                    using (d) { throw new Exception(); }
                } catch (Exception) { }
#if DEBUG
                Assert.True(d.DEBUG_exceptionWasDetected);
#endif
            }
            {
                await TestErrorDetectionWithMultipleThreads(() => { throw new Exception(); });
                await TestErrorDetectionWithMultipleThreads(async () => { throw new Exception(); });
                await TestErrorDetectionWithMultipleThreads(() => { return Task.FromException(new Exception()); });
            }
        }

        [Obsolete("This test tests a method that only should be used for debugging and is permitted to be used in release builds")]
        private static async Task TestErrorDetectionWithMultipleThreads(Func<Task> newErrorTask) {
            var d = new MyDisposable();
            using (d) {
                Task t1 = TaskV2.Run(newErrorTask);
                Task t2 = null, t3 = null;
                try {
                    Assert.False(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
                    t2 = TaskV2.Run(newErrorTask);
                    Assert.False(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
                    t3 = TaskV2.Run(newErrorTask);
                    Assert.False(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
                    await Task.WhenAll(t1, t2, t3);
                } catch (Exception) {
#if DEBUG
                    Assert.True(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
#endif
                }
                Assert.False(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
                Assert.True(t1.IsCompleted);
                Assert.True(t2.IsCompleted);
                Assert.True(t3.IsCompleted);
                Assert.True(t1.IsFaulted);
                Assert.True(t2.IsFaulted);
                Assert.True(t3.IsFaulted);
            }
            Assert.False(d.DEBUG_ThrownExceptionDetectedInCurrentContext());
            Assert.False(d.DEBUG_exceptionWasDetected);
        }

    }

}