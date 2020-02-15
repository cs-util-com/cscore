using com.csutil;
using System;
using Xunit;

namespace com.AaaaaRenameMeAaaaa.tests {

    public class ExampleUnitTest1 {

        [Fact]
        public void Test1() {
            // Test using classes from the main lib:
            var x = new AaaaaRenameMeAaaaa_ExampleClass1("Abc");
            Assert.True(x.isTrue());

            // Test using classes from cscore:
            var json = JsonWriter.GetWriter().Write(x);
            var x2 = JsonReader.GetReader().Read<AaaaaRenameMeAaaaa_ExampleClass1>(json);
            Assert.Equal(x.name, x2.name);
        }
    }

}
