using System;
using System.Threading.Tasks;
using com.csutil.datastructures;
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
        public void ReferenceExampleWithPrimitives() {

            Double a = 123;
            Double b = 123;
            Assert.Equal(a, b); // Both have an equal value
            // They dont share the same reference in memory:
            Assert.NotSame(a, b);
            // For primitives c=b will copy the memory:
            Double c = b;
            // They still dont have the same reference:
            Assert.NotSame(b, c);

            string s1 = "123";
            string s2 = "1" + "2" + "3";
            Assert.Equal(s1, s2);
            // For strings the compiler optimizes memory:
            Assert.Same(s1, s2);

        }

        [Fact]
        public void ReferenceExampleWithObjects() {

            Object a = new Object();
            Object b = new Object();
            Assert.NotEqual(a, b);
            Assert.NotSame(a, b);
            // Point var b and c on the same object:
            Object c = b;
            Assert.Same(b, c);
            // They are equal because they are the same:
            Assert.Equal(b, c);

        }

        [Fact]
        public void DateTime_Examples() {

            // Parse a Unix timestamp into a DateTime object:
            DateTime myDateTime = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889000);

            // Create a compromise of a human readable sting that is usable for file names etc:
            Assert.Equal("2019-01-15_07.04", myDateTime.ToReadableString());

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
                if (AssertV2.throwExeptionIfAssertionFails) { return; } // Abort test if the flag is not correct
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
        public void TestDynamicInvokeV2() {
            var wasCalled = false;
            Action<string, int> a = (s, i) => { wasCalled = true; };
            {
                object result;
                object[] inputArguments = new object[] { "a", 1 };
                Assert.True(a.DynamicInvokeV2(inputArguments, out result, true));
                Assert.True(wasCalled);
                wasCalled = false;
            }
            { // Passing to many parameters will result in the parameters being ignored:
                object result;
                object[] inputArguments = new object[] { "a", 1, "b" };
                Assert.True(a.DynamicInvokeV2(inputArguments, out result, true));
                Assert.True(wasCalled);
                wasCalled = false;
            }
            { // Test that false is returned when throwIfNotEnoughParams is false:
                object result;
                object[] inputArguments = new object[] { "a" };
                Assert.False(a.DynamicInvokeV2(inputArguments, out result, throwIfNotEnoughParams: false));
                Assert.False(wasCalled);
            }
            { // Test that error is thrown when throwIfNotEnoughParams is true:
                object result;
                object[] inputArguments = new object[] { "a" };
                Assert.Throws<ArgumentException>(() => {
                    a.DynamicInvokeV2(inputArguments, out result, throwIfNotEnoughParams: true);
                });
            }
            { // Test that error is thrown when incorrect parameter types are used:
                object result;
                object[] inputArguments = new object[] { "a", "b" };
                Assert.Throws<ArgumentException>(() => {
                    a.DynamicInvokeV2(inputArguments, out result, throwIfNotEnoughParams: true);
                });
            }
        }

        [Fact]
        public async Task TestTaskThrowIfException() {
            Task myFailedTask = CreateAndRunATaskThatFails();
            while (!myFailedTask.IsCompleted) { await TaskV2.Delay(5); }
            Assert.Throws<AggregateException>(() => {
                myFailedTask.ThrowIfException(); // the task failed so this will throw
            });
        }

        private static async Task CreateAndRunATaskThatFails() {
            Task myFailedTask = null;
            await Assert.ThrowsAsync<AggregateException>(async () => {
                myFailedTask = TaskV2.Run(() => { throw new Exception("Some error"); });
                await myFailedTask;
                Assert.True(myFailedTask.IsFaulted);
                Assert.True(myFailedTask.IsCompleted);
            });
            Assert.NotNull(myFailedTask);
            Assert.NotNull(myFailedTask.Exception);
        }

    }

}