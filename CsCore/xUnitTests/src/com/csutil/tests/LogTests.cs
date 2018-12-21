using System;
using System.Threading;
using Xunit;

namespace com.csutil.tests {
    public class LogTests {
        [Fact]
        public void Test1() {
            Log.d("This is a log");
            Log.w("This is a warning", 123);
            Log.e("This is an error", 123, 123);
            Log.e(new Exception("I am an exception"), 123, 123, 123);

            AssertV2.IsTrue(1 + 1 == 4, "1+1 is not 4");
            MyCustomMethod123("aa", 22);

            AssertV2.AreEqual(1, 2);
            var s1 = "a";
            AssertV2.AreNotEqual(s1, s1, "s1");
            AssertV2.AreNotEqual(1, 1);
            AssertV2.IsNull("I am myVarX and I am not null", "myVarX");

        }

        private static void MyCustomMethod123(string x, int i) {
            var t = AssertV2.TrackTiming();
            Thread.Sleep(10);
            t.AssertUnderXms(20);
            Assert.True(t.IsUnderXms(20));
        }
    }
}