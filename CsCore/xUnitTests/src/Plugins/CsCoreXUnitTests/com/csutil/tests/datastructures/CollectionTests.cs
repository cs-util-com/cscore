using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests {

    public class CollectionTests {

        public CollectionTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void DictionaryExtensions_Examples() {

            var myDictionary = new Dictionary<string, string>();
            // Add an entry s1 with value "a":
            Assert.Null(myDictionary.AddOrReplace("s1", "a"));

            // Replacing the value of s1 with b:
            Assert.Equal("a", myDictionary.AddOrReplace("s1", "b"));

            // The replaced value "b" is returned by the method:
            Assert.Equal("b", myDictionary.AddOrReplace("s1", "a"));

        }

        [Fact]
        public void FixedSizedQueue_Examples() {

            // A queue with a fixed maximum size:
            var queue = new FixedSizedQueue<string>(3);

            // The queue is filled with 3 values:
            queue.Enqueue("a");
            queue.Enqueue("b");
            queue.Enqueue("c");
            Assert.Equal(3, queue.Count);

            // If the queue is filled with a 4th value the oldes will be dropped:
            queue.Enqueue("d");
            Assert.Equal(3, queue.Count); // "a" was dropped from the queue

            // The first entry in the queue will be "b" because "a" was dropped:
            Assert.Equal("b", queue.Dequeue());
            Assert.Equal("c", queue.Dequeue());
            Assert.Equal("d", queue.Dequeue());
            // Now the queue is emtpy and will return null when dequeued:
            Assert.Null(queue.Dequeue());

        }

        [Fact]
        public void IEnumerableExtensions_Examples() {

            List<string> myList = null;
            // If the List is null this will not throw a nullpointer exception:
            Assert.True(myList.IsNullOrEmpty());
            Assert.Equal("null", myList.ToStringV2((s) => s));

            myList = new List<string>();
            Assert.True(myList.IsNullOrEmpty());
            Assert.Equal("()", myList.ToStringV2((s) => s, bracket1: "(", bracket2: ")"));

            myList.Add("s1");
            Assert.False(myList.IsNullOrEmpty());
            Assert.Equal("{s1}", myList.ToStringV2((s) => s, bracket1: "{", bracket2: "}"));

            myList.Add("s2");
            Assert.False(myList.IsNullOrEmpty());
            Assert.Equal("[s1, s2]", myList.ToStringV2((s) => s, bracket1: "[", bracket2: "]"));

        }

        [Fact]
        public void Filter_Map_Reduce_Examples() {

            IEnumerable<string> myStrings = new List<string>() { "1", "2", "3", "4", "5" };
            IEnumerable<int> convertedToInts = myStrings.Map(s => int.Parse(s));
            IEnumerable<int> filteredInts = convertedToInts.Filter(i => i <= 3); // Keep 1,2,3
            Assert.False(filteredInts.IsNullOrEmpty());
            Log.d("Filtered ints: " + filteredInts.ToStringV2(i => "" + i)); // "[1, 2, 3]"
            int sumOfAllInts = filteredInts.Reduce((sum, i) => sum + i); // Sum up all ints
            Assert.Equal(6, sumOfAllInts); // 1+2+3 is 6

        }

        [Fact]
        public void IEnumerableTests() {
            var l_123 = new List<string>() { "1", "2", "3" };
            var l_ABC = new List<string>() { "A", "B", "C" };
            {
                var l_1ABC23 = l_123.InsertRangeViaUnion(1, l_ABC);
                Assert.Equal("1", l_1ABC23.First());
                Assert.Equal("3", l_1ABC23.Last());
                Assert.Equal(1, l_1ABC23.IndexOf("A"));
                Assert.Equal(-1, l_1ABC23.IndexOf("D"));
                Assert.Equal(0, l_1ABC23.IndexOf("1"));
                Assert.Equal(5, l_1ABC23.IndexOf("3"));

                var normalInsertRangeList = new List<string>() { "1", "2", "3" };
                normalInsertRangeList.InsertRange(1, l_ABC);
                Assert.Equal(l_1ABC23, normalInsertRangeList);
            }
            Assert.Equal("A", l_123.InsertRangeViaUnion(index: 0, items: l_ABC).First());
            Assert.Equal("C", l_123.InsertRangeViaUnion(index: 999, items: l_ABC).Last());
        }

        [Fact]
        public void TestRecursiveTreeFlattenTraversal() {

            var tree = new TreeNode() {
                id = "Root",
                children = new TreeNode[] {
                    new TreeNode() { id = "1 - 1", children = new TreeNode[] {
                            new TreeNode() { id = "1 - 1 - 1" },
                            new TreeNode() { id = "1 - 1 - 2" }
                        }
                    },
                    new TreeNode() { id = "1 - 2" },
                    new TreeNode() { id = "1 - 3" }
                }
            };

            var listDepthFi = TreeFlattenTraverse.DepthFirst(tree, x => x.children).ToList();
            var listBreadth = TreeFlattenTraverse.BreadthFirst(tree, x => x.children).ToList();

            Assert.Equal(6, listDepthFi.Count);
            Assert.Equal(6, listBreadth.Count);

            Assert.Equal("Root", listDepthFi[0].id);
            Assert.Equal("Root", listBreadth[0].id);

            Assert.Equal("1 - 1", listDepthFi[1].id);
            Assert.Equal("1 - 1", listBreadth[1].id);

            Assert.Equal("1 - 1 - 1", listDepthFi[2].id);
            Assert.Equal("1 - 2", listBreadth[2].id);

            Assert.Equal("1 - 3", listDepthFi.Last().id);
            Assert.Equal("1 - 1 - 2", listBreadth.Last().id);

        }

        [Fact]
        public void TestMove() {
            TestMoveWith(new List<string>() { "A", "B", "C", "D" });
            TestMoveWith(new string[] { "A", "B", "C", "D" });
        }

        private static void TestMoveWith(IList<string> l) {
            l.Move(2, 3);
            Assert.Equal("[A, B, D, C]", l.ToStringV2());
            l.Move(3, 0);
            Assert.Equal("[C, A, B, D]", l.ToStringV2());
            l.Move(1, 3);
            Assert.Equal("[C, B, D, A]", l.ToStringV2());
            var r = new Random();
            for (int i = 0; i < 1000; i++) { l.Move(r.Next(0, l.Count), r.Next(0, l.Count)); }
            Assert.Throws<ArgumentOutOfRangeException>(() => { l.Move(0, 4); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { l.Move(0, -1); });
        }

        private class TreeNode {
            public string id;
            public TreeNode[] children;
            public override string ToString() { return id; }
        }

        [Fact]
        public void TestToHashSet() {
            var h = com.csutil.netstandard2_1polyfill.IEnumerableExtensions.ToHashSet(new List<string>() { "A", "B", "A", "C" });
            Assert.Equal(3, h.Count);
        }

        [Fact]
        public void TestDictionaryExtensions() {
            {
                var a = new Dictionary<string, string>() { { "key1", "a" }, { "key2", "b" } };
                var b = new Dictionary<string, string>() { { "key1", "a" } };
                Assert.Equal("[key1, a]", a.Intersect(b).Single().ToString());
                Assert.Equal("[key2, b]", a.ExceptKeys(b).Single().ToString());
            }
            {
                var a = new HashSet<string>() { "a", "b" };
                var b = new HashSet<string>() { "c", "d" };
                Assert.True(a.AddRange(b));
                Assert.Equal("[a, b, c, d]", a.ToStringV2(x => "" + x));
                Assert.False(a.AddRange(b));
                Assert.Equal("[a, b, c, d]", a.ToStringV2(x => "" + x));
            }
        }

        [Fact]
        public async Task PromiseMap_ExampleUsage1() {

            var promiseMapCache = new PromiseMap<string, string>();

            // Register a computation task in the promise map for anyone to access via the map
            promiseMapCache["a"] = ComputeResultForA();

            var s = StopwatchV2.StartNewV2();
            Assert.Equal("1", await promiseMapCache["a"]);
            var firstComputeTime = s.ElapsedMilliseconds;
            Assert.True(firstComputeTime >= 100, "firstComputeTime=" + firstComputeTime);
            Assert.True(s.IsRunning); // Keep the stopwatch running 

            // The second time accessing the same computation task is completed instantly:
            Assert.Equal("1", await promiseMapCache["a"]);
            Assert.True(s.ElapsedMilliseconds <= firstComputeTime + 1, $"s.ElapsedMilliseconds {s.ElapsedMilliseconds} > firstComputeTime {firstComputeTime} + 5ms");

        }

        private async Task<string> ComputeResultForA() {
            await TaskV2.Delay(200); // Simulates an expensive computation
            return "1";
        }

        [Fact]
        public async Task TestCountIsBelowAndCountIsAbove1() {
            var x = new List<int>();
            AssertIsEnumerableWith0Entries(x);
            x.Add(123);
            AssertIsEnumerableWith1Entries(x);
            x.Add(456);
            AssertIsEnumerableWith2Entries(x);
        }

        [Fact]
        public async Task TestCountIsBelowAndCountIsAbove2() {
            AssertIsEnumerableWith0Entries(NewVerySlowEnumerableWithEntries(0));
            AssertIsEnumerableWith1Entries(NewVerySlowEnumerableWithEntries(1));
            AssertIsEnumerableWith2Entries(NewVerySlowEnumerableWithEntries(2));
        }

        [Fact]
        public async Task TestCountIsBelowAndCountIsAbove3() {
            var e = NewVerySlowEnumerableWithEntries(10);
            var t1 = Log.MethodEnteredWith("Using .Count()");
            Assert.True(e.Count() > 2);
            Assert.False(e.Count() < 2);
            Log.MethodDone(t1);
            var t2 = Log.MethodEnteredWith("Using .CountIsAbove and .CountIsBelow");
            Assert.True(e.CountIsAbove(2));
            Assert.False(e.CountIsBelow(2));
            Log.MethodDone(t2);

            // The CountIsAbove and CountIsBelow must be much faster then using .Count():
            Assert.True(t1.ElapsedMilliseconds > 2 * t2.ElapsedMilliseconds);
        }

        private IEnumerable<int> NewVerySlowEnumerableWithEntries(int count) {
            for (int i = 0; i < count; i++) {
                Thread.Sleep(50);
                yield return i + 1000;
            }
        }

        private static void AssertIsEnumerableWith0Entries(IEnumerable x) {
            Assert.True(x.CountIsAbove(-1));
            Assert.False(x.CountIsAbove(0));
            Assert.True(x.CountIsBelow(1));
            Assert.False(x.CountIsBelow(0));
        }

        private static void AssertIsEnumerableWith1Entries(IEnumerable x) {
            Assert.True(x.CountIsBelow(2));
            Assert.False(x.CountIsBelow(1));
            Assert.True(x.CountIsAbove(0));
            Assert.False(x.CountIsAbove(1));
        }

        private static void AssertIsEnumerableWith2Entries(IEnumerable x) {
            Assert.True(x.CountIsBelow(3));
            Assert.False(x.CountIsBelow(2));
            Assert.True(x.CountIsAbove(1));
            Assert.False(x.CountIsAbove(2));
        }

    }

}