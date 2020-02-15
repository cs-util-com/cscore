using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace com.csutil.tests.datastructures {

    public class ImmutableListTests {

        public ImmutableListTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            {
                var elemCount = 100000;
                var t1 = RunOnList("add", elemCount, (list) => { list.Add(1); });
                var t2 = RunOnImmutableList("ExampleUsage1.add", elemCount, (list) => list.Add(1));
                var timeDiff = Math.Abs(t1 - t2);
                Assert.True(timeDiff < 2000, "add t1=" + t1 + ", t2=" + t2 + ", timeDiff=" + timeDiff);
            }
            {
                var elemCount = 100000;
                var t1 = RunOnList("insert", elemCount, (list) => { list.Insert(0, 1); });
                var t2 = RunOnImmutableList("ExampleUsage1.insert", elemCount, (list) => list.Insert(0, 1));
                var timeDiff = Math.Abs(t1 - t2);
                Assert.True(timeDiff < 5000, "insert t1=" + t1 + ", t2=" + t2 + ", timeDiff=" + timeDiff);
            }
            {
                var elemCount = 100000;
                List<int> l1 = new List<int>();
                RunOnList("fill", l1, elemCount, l => l.Add(1));
                Assert.Equal(elemCount, l1.Count);
                var t1 = RunOnList("ExampleUsage1.remove", l1, elemCount, (list) => { list.RemoveAt(0); });
                Assert.Empty(l1);

                var l2 = ImmutableList.Create<int>();
                RunOnImmutableList("fill", ref l2, elemCount, (list) => list.Add(1));
                Assert.Equal(elemCount, l2.Count);
                var t2 = RunOnImmutableList("ExampleUsage1.remove", ref l2, elemCount, (list) => list.RemoveAt(0));
                Assert.Empty(l2);
                var timeDiff = Math.Abs(t1 - t2);
                Assert.True(timeDiff < 4000, "remove t1=" + t1 + ", t2=" + t2 + ", timeDiff=" + timeDiff);
            }

        }

        private static long RunOnImmutableList(string operationName, int elemCount, Func<ImmutableList<int>, ImmutableList<int>> f) {
            var l = ImmutableList.Create<int>();
            return RunOnImmutableList(operationName, ref l, elemCount, f);
        }

        private static long RunOnImmutableList(string operationName, ref ImmutableList<int> l, int elemCount, Func<ImmutableList<int>, ImmutableList<int>> f) {
            var t = Log.MethodEntered(operationName, "elemCount=" + elemCount);
            for (int i = 0; i < elemCount; i++) { l = f(l); }
            Log.MethodDone(t);
            return t.ElapsedMilliseconds;
        }

        private static long RunOnList(string operationName, int elemCount, Action<List<int>> f) {
            return RunOnList(operationName, new List<int>(), elemCount, f);
        }

        private static long RunOnList(string operationName, List<int> l, int elemCount, Action<List<int>> f) {
            var t = Log.MethodEntered(operationName, "elemCount=" + elemCount);
            for (int i = 0; i < elemCount; i++) { f(l); }
            Log.MethodDone(t);
            return t.ElapsedMilliseconds;
        }
    }

}