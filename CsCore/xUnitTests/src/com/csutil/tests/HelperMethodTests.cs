using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using com.csutil.datastructures;
using com.csutil.encryption;
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
        public void DictionaryExtensions_Examples() {
            var dic = new Dictionary<string, string>();
            Assert.Null(dic.AddOrReplace("s1", "a"));
            Assert.Equal("a", dic.AddOrReplace("s1", "b"));
            Assert.Equal("b", dic.AddOrReplace("s1", "a"));
        }

        [Fact]
        public void IEnumerableExtensions_Examples() {
            List<string> myList = null;

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
        public void ChangeTrackerUsage_Example1() {
            // The ChangeTracker tracks if a variable changes:
            var myChangeTracker = new ChangeTracker<string>("a"); // init its value with "a"
            // Switch its value form "a" to "b"
            Assert.True(myChangeTracker.setNewValue("b"));
            // If "b" is set again, the change tracker return false:
            Assert.False(myChangeTracker.setNewValue("b"));
            Assert.Equal("b", myChangeTracker.value);
        }

        [Fact]
        public void FixedSizedQueue_Example1() {
            // A queue with a fixed maximum size:
            var q = new FixedSizedQueue<string>(3);

            // The queue is filled with 3 values:
            q.Enqueue("a").Enqueue("b").Enqueue("c");
            Assert.Equal(3, q.Count);

            // If the queue is filled with a 4th value the oldes will be dropped:
            q.Enqueue("d");
            Assert.Equal(3, q.Count); // "a" was dropped from the queue

            // The first entry in the queue will be "b" because "a" was dropped:
            Assert.Equal("b", q.Dequeue());
        }

        [Fact]
        public void DateTime_Example1() {
            DateTime myDate = DateTimeParser.NewDateTimeFromUnixTimestamp(1547535889000);
            // Create a compromise of a human readable sting that is usable for file names etc:
            Assert.Equal("2019-01-15_07.04", myDate.ToReadableString());
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
            });
        }

        [Fact]
        public void StringExtension_Examples() {
            string myString = "abc";

            // myString.Substring(..) examples:
            Assert.Equal("bc", myString.Substring(1, "d", includeEnd: true));
            Assert.Equal("bc", myString.Substring(1, "c", includeEnd: true));
            Assert.Equal("ab", myString.Substring("c", includeEnd: false));

            // myString.SubstringAfter(..) examples:
            myString = "[{a}]-[{b}]";
            Assert.Equal("a}]-[{b}]", myString.SubstringAfter("{"));
            Assert.Equal("{b}]", myString.SubstringAfter("[", startFromBack: true));
            Assert.Throws<Exception>(() => { myString.SubstringAfter("("); });

            // Often SubstringAfter and Substring are used in combination:
            myString = "[(abc)]";
            Assert.Equal("abc", myString.SubstringAfter("(").Substring(")", includeEnd: false));
        }

        [Fact]
        public void StringEncryptionTests() {
            var myString = "some text..";

            // Encrypt myString with the password "123":
            var myEncryptedString = myString.Encrypt("123");

            // The encrypted string is different to myString:
            Assert.NotEqual(myString, myEncryptedString);
            // Encrypting with a different password results into another encrypted string:
            Assert.NotEqual(myEncryptedString, myString.Encrypt("124"));

            // Decrypt the encrypted string back with the correct password:
            Assert.Equal(myString, myEncryptedString.Decrypt("123"));

            // Using the wrong password results in an exception:
            Assert.Throws<CryptographicException>(() => {
                Assert.NotEqual(myString, myEncryptedString.Decrypt("124"));
            });
        }

        [Fact]
        public void TaskExtensionTests() {
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

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 { }
        private class MySubClass2 : MyClass1 { }

    }
}