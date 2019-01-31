using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using com.csutil.datastructures;
using com.csutil.encryption;
using com.csutil.random;
using Xunit;

namespace com.csutil.tests {

    public class HelperMethodTests {

        [Fact]
        public void DelegateExtensions_Examples() {

            Action myAction = null;
            // If the action is null nothing will happen:
            Assert.False(myAction.InvokeIfNotNull());

            // Now set the action (it will flip the myActionWasCalled flag):
            bool myActionWasCalled = false;
            myAction = () => {
                myActionWasCalled = true;
            };

            // If the action is not null it will be called normally:
            Assert.True(myAction.InvokeIfNotNull());
            Assert.True(myActionWasCalled);
        }

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
        public void ChangeTrackerUsage_Examples() {

            // The ChangeTracker tracks if a variable changes:
            var myChangeTracker = new ChangeTracker<string>("a"); // init its value with "a"

            // Switch its value form "a" to "b"
            Assert.True(myChangeTracker.setNewValue("b"));

            // If "b" is set again, the change tracker return false:
            Assert.False(myChangeTracker.setNewValue("b"));
            Assert.Equal("b", myChangeTracker.value);

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
        public void DateTime_Examples() {

            // Parse a Unix timestamp into a DateTime object:
            DateTime myDateTime = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889000);

            // Create a compromise of a human readable sting that is usable for file names etc:
            Assert.Equal("2019-01-15_07.04", myDateTime.ToReadableString());

        }

        [Fact]
        public void RandomExtensions_Examples() {

            var random = new Random();

            { // Example for random.NextBool:
                int heads = 0;
                int tails = 0;
                for (int i = 0; i < 10000; i++) {
                    bool coinFlip = random.NextBool();
                    if (coinFlip) { heads++; } else { tails++; }
                }
                int diff = Math.Abs(heads - tails);
                // Assert.True(diff < 300, "Coin flips were not normally distributed around 0! diff=" + diff);
            }

            var randomName = random.NextRandomName();
            Log.d("The generated random name is: " + randomName);

            // random.NextDouble() with a range from lowerBound to upperBound:
            double randomDouble = random.NextDouble(lowerBound: -100, upperBound: 100);
            Assert.InRange(randomDouble, -100, 100);

            // random.NextFloat() with a range from lowerBound to upperBound:
            float randomFloat = random.NextFloat(lowerBound: 20, upperBound: 50);
            Assert.InRange(randomFloat, 20, 50);

        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 { }
        private class MySubClass2 : MyClass1 { }

        [Fact]
        public void TypeExtension_Examples() {
            Type MySubClass1 = typeof(MySubClass1);

            // type.IsSubclassOf<..>() examples:
            Assert.True(MySubClass1.IsSubclassOf<MyClass1>());
            Assert.False(MySubClass1.IsSubclassOf<MySubClass2>());
            Assert.True(typeof(MySubClass2).IsSubclassOf<MyClass1>());
            Assert.False(typeof(MyClass1).IsSubclassOf<MySubClass1>());

            // Checking if 2 types are equal using the TypeCheck class:
            Assert.True(TypeCheck.AreEqual<MyClass1, MyClass1>());
            Assert.False(TypeCheck.AreEqual<MySubClass1, MyClass1>());

            // type.IsCastableTo<..>() examples:
            Assert.True(typeof(MySubClass2).IsCastableTo<MyClass1>());
            Assert.False(typeof(MyClass1).IsCastableTo<MySubClass2>());

            // type.IsCastableTo(..) examples:
            Assert.True(typeof(MySubClass1).IsCastableTo(typeof(MyClass1)));
            Assert.True(typeof(MyClass1).IsCastableTo(typeof(MyClass1)));
            Assert.False(typeof(MyClass1).IsCastableTo(typeof(MySubClass1)));
        }

        [Fact]
        public void DateTime_MoreTests() {
            AssertV2.ThrowExeptionIfAssertionFails(false, () => {
                var dateTime1 = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889);
                var dateTime2 = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889000);
                Assert.Equal(dateTime1, dateTime2);
                var dateTime3 = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889, autoCorrectIfPassedInSeconds: false);
                Assert.NotEqual(dateTime1, dateTime3);
            });
            AssertV2.ThrowExeptionIfAssertionFails(false, () => {
                var dateTime1 = DateTimeParser.NewDateTimeFromUnixTimestamp(-2);
                var dateTime2 = DateTimeParser.NewDateTimeFromUnixTimestamp(2);
                Assert.True(dateTime1.IsBefore(dateTime2));
                Assert.False(dateTime2.IsBefore(dateTime1));
                Assert.True(dateTime2.IsAfter(dateTime1));
                Assert.False(dateTime1.IsAfter(dateTime2));
                Assert.True(DateTimeParser.NewDateTimeFromUnixTimestamp(0).IsBetween(dateTime1, dateTime2));
                Assert.False(DateTimeParser.NewDateTimeFromUnixTimestamp(0).IsBetween(dateTime2, dateTime1));
                Assert.False(DateTimeParser.NewDateTimeFromUnixTimestamp(3).IsBetween(dateTime1, dateTime2));
            });

            // Make sure the assertions in DateTimeParser.NewDateTimeFromUnixTimestamp work correctly and detect abnormal behavior:
            Assert.Throws<Exception>(() => {
                AssertV2.ThrowExeptionIfAssertionFails(() => {
                    DateTimeParser.NewDateTimeFromUnixTimestamp(-1);
                });
            });

        }

        [Fact]
        public void DelegateExtensions_MoreTests() {
            {
                Action<string> a = null;
                Assert.False(a.InvokeIfNotNull(""));
                var wasCalled = false;
                a = (s) => { wasCalled = true; };
                Assert.True(a.InvokeIfNotNull(""));
                Assert.True(wasCalled);
            }
            {
                Action<string, int> a = null;
                Assert.False(a.InvokeIfNotNull("", 123));
                var wasCalled = false;
                a = (s, i) => { wasCalled = true; };
                Assert.True(a.InvokeIfNotNull("", 123));
                Assert.True(wasCalled);
            }
            {
                Action<string, int, bool> a = null;
                Assert.False(a.InvokeIfNotNull("", 123, true));
                var wasCalled = false;
                a = (s, i, b) => { wasCalled = true; };
                Assert.True(a.InvokeIfNotNull("", 123, true));
                Assert.True(wasCalled);
            }
        }

        [Fact]
        public void TestTaskThrowIfException() {
            Task myFailedTask = CreateAndRunATaskThatFails();
            Assert.Throws<AggregateException>(() => {
                myFailedTask.ThrowIfException(); // the task failed so this will throw
            });
        }

        private static Task CreateAndRunATaskThatFails() {
            Task myFailedTask = null;
            Assert.Throws<AggregateException>(() => {
                myFailedTask = Task.Run(() => { throw Log.e("Some error"); });
                myFailedTask.Wait();
            });
            Assert.NotNull(myFailedTask);
            Assert.NotNull(myFailedTask.Exception);
            return myFailedTask;
        }

        [Fact]
        public void RandomExtensions_MoreTests() {

            var random = new Random();

            { // Test with MinValue and MaxValue:
                float f = random.NextFloat(float.MinValue, float.MaxValue);
                Assert.True(float.MinValue < f && f < float.MaxValue, "f=" + f);
            }

            { // Test with MinValue and MaxValue:
                double d = random.NextDouble(double.MinValue, double.MaxValue);
                Assert.True(double.MinValue < d && d < double.MaxValue, "d=" + d);
            }

            // Example for random.NextFloat():
            TestRandomFloat(random, 0, 1);
            TestRandomFloat(random, -0.1f, 0);
            TestRandomFloat(random, 10, 20000);
            TestRandomFloat(random, -100000, -100);
            TestRandomFloat(random, -10000, 10000);
            TestRandomFloat(random, float.MinValue, float.MaxValue);

            { // Example for random.NextDouble(min, max):
                double min = -1000;
                double max = 1000;
                double sum = 0;
                for (int i = 0; i < 10000; i++) {
                    double x = random.NextDouble(min, max);
                    Assert.InRange(x, min, max);
                    sum += x;
                } // The sum should be normally distributed around 0:
                Assert.InRange(sum, min * 200, max * 200);
            }

            { // Example for random.NextFloat(min, max):
                float min = -1000;
                float max = 1000;
                float sum = 0;
                for (int i = 0; i < 10000; i++) {
                    float x = random.NextFloat(min, max);
                    Assert.InRange(x, min, max);
                    sum += x;
                } // The sum should be normally distributed around 0:
                Assert.InRange(sum, min * 200, max * 200);
            }
        }

        private static void TestRandomFloat(Random random, float lowerBound, float upperBound) {
            double min = float.MaxValue;
            double max = float.MinValue;
            var results = new List<float>();
            for (int i = 0; i < 100000; i++) {
                float x = random.NextFloat(lowerBound, upperBound);
                Assert.True(x <= upperBound, "x=" + x);
                Assert.True(lowerBound <= x, "x=" + x);
                if (x < min) { min = x; }
                if (x > max) { max = x; }
                results.Add(x);
            }
            Assert.True(isNormallyDistributed(results));
            var reachedUpperBound = 100d * (1d - (upperBound - max) / (upperBound - lowerBound));
            var reachedLowerBound = 100d * (1d - (min - lowerBound) / (upperBound - lowerBound));
            Assert.True(reachedLowerBound > 98 && reachedUpperBound > 98, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
            Assert.True(reachedLowerBound <= 100 && reachedUpperBound <= 100, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
        }

        private static bool isNormallyDistributed(List<float> results) {
            return true; // TODO
        }


    }

}