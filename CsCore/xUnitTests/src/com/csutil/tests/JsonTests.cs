using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests {
    public class JsonTests : IDisposable {
        public JsonTests() { // Setup before each test
        }
        public void Dispose() { // TearDown after each test
        }

        [Fact]
        public void TestClassWithoutDefaultConstructor() {
            var x1 = new MySubClass1("I am myString");
            var json = JsonWriter.GetWriter().Write((MyClass1)x1);
            var x2 = JsonReader.GetReader().Read<MySubClass1>(json);
            Assert.Equal(x1.myString, x2.myString);
        }

        [Fact]
        public void TestMissingFieldInClass() {
            var x1 = new MySubClass2() { myString = "I am s1", myString2 = "I am s2" };
            var json = JsonWriter.GetWriter().Write((MyClass1)x1);
            var x2 = JsonReader.GetReader().Read<MySubClass1>(json);
            Assert.Equal(x1.myString, x2.myString);
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 {
            public string myString;
            public MySubClass1(string s) : base() { myString = s; }
        }
        private class MySubClass2 : MyClass1 {
            public string myString;
            public string myString2;
        }

    }
}
