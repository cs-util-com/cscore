using System;
using System.Collections.Generic;
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

    }

}