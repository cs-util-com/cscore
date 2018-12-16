using System;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests {
    public class JsonTests : IDisposable {

        public JsonTests() {
            // Setup before each test
        }

        public void Dispose() {
            // TearDown after each test
        }

        [Fact]
        public void Test1() {

            var serializer = new JsonSerializer();

            // var r = JsonReader.NewReader();
            // var w = JsonWriter.NewWriter();

        }

        private class MyClass1 {
            internal MyClass1() {

            }

        }
        private class MySubClass1 : MyClass1 {

            string s;
            public int i;
            private bool b;

            public MySubClass1(string s) : base() {
                Log.d("MySubClass1 created with s=" + s);
            }

        }

    }
}
