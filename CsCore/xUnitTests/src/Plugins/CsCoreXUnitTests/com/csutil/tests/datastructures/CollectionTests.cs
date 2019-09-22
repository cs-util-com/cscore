using System.Collections.Generic;
using System.Linq;
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

    }
}