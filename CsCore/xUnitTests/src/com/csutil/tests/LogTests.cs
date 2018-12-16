using System;
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
        }
    }
}