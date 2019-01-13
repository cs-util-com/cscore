using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.csutil.json;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests {

    public class JsonTests {

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

        [Fact]
        public void TestMissingFieldInClass2() {
            var x1 = new MySubClass2() { myString = "I am s1", myString2 = "I am s2" };
            var json = JsonWriter.GetWriter().Write(x1);
            Log.d("json=" + json);
            var x3 = JsonReader.GetReader().Read<MySubClass3>(json);
            Assert.Equal(x1.myString, x3.myString);
        }

        private class MyClass1 { }
        private class MySubClass1 : MyClass1 {
            public string myString;

            //The class has no default constructor but still can be instantiated by the JSON logic
            public MySubClass1(string s) : base() { myString = s; }
        }

        private class MySubClass2 : MyClass1 {
            public string myString;
            public string myString2;
        }

        private class MySubClass3 : MyClass1, HandleAdditionalJsonFields {
            public string myString;

            private Dictionary<string, object> missingFields;
            public Dictionary<string, object> GetMissingFields() { return missingFields; }
            public void SetMissingFields(Dictionary<string, object> missingFields) { this.missingFields = missingFields; }
        }

    }
}
