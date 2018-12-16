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
            var x1 = new MySubClass1("test");
            var json = JsonWriter.NewWriter().Write((MyClass1)x1);
            var x2 = JsonReader.NewReader().Read<MySubClass1>(json);
            Assert.Equal(x1.s, x2.s);
        }

        [Fact]
        public void TestMissingFieldInClass() {
            var x1 = new MySubClass2() { s = "s", s2 = "s2" };
            var json = JsonWriter.NewWriter().Write((MyClass1)x1);
            var x2 = JsonReader.NewReader().Read<MySubClass1>(json);
            Assert.Equal(x1.s, x2.s);
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 {
            public string s;
            public MySubClass1(string s) : base() { this.s = s; }
        }
        private class MySubClass2 : MyClass1 {
            public string s;
            public string s2;
        }


    }
}
