using System;
using Xunit;

namespace com.csutil.tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var x = new MyClass1();
            Assert.True(x.isTrue());
            Assert.Equal(4, 2 + 2);
        }
    }
}
