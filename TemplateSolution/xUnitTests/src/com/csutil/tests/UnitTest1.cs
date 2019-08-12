using System;
using Xunit;

namespace com.csutil.tests {

    public class UnitTest1 {

        [Fact]
        public void Test1() {
            // Test using classes from the main lib:
            var x = new MyClass1("Abc");
            Assert.True(x.isTrue());

            // Test using classes from cscore:
            var json = JsonWriter.GetWriter().Write(x);
            var x2 = JsonReader.GetReader().Read<MyClass1>(json);
            Assert.Equal(x.name, x2.name);
        }
    }

}
