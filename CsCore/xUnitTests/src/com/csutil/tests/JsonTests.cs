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
        public void Test1() {
            MySubClass1 x1 = new MySubClass1("test");
            var json = JsonWriter.NewWriter().Write((MyClass1)x1);
            var x2 = JsonReader.NewReader().Read<MySubClass1>(json);
            Assert.Equal(x1.s, x2.s);
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 {
            public string s;
            public MySubClass1(string s) : base() { this.s = s; }
        }

    }
}
