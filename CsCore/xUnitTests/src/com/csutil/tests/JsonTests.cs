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
            var x1 = new MySubClassWithoutADefaultConstructor("I am myString");
            var json = JsonWriter.GetWriter().Write((MyClass1)x1);
            var x2 = JsonReader.GetReader().Read<MySubClassWithoutADefaultConstructor>(json);
            Assert.Equal(x1.myString, x2.myString);
        }

        [Fact]
        public void TestMissingFieldInClass() {
            var x1 = new MySubClass1() { myString = "I am s1", myString2 = "I am s2" };
            var jsonString = JsonWriter.GetWriter().Write(x1 as MyClass1);
            var x2 = JsonReader.GetReader().Read<MySubClassWithoutADefaultConstructor>(jsonString);
            Assert.Equal(x1.myString, x2.myString);
        }

        [Fact]
        public void TestMissingFieldInClass2() {
            var x1 = new MySubClass1() { myString = "I am s1", myString2 = "I am s2" };
            var jsonString = JsonWriter.GetWriter().Write(x1);
            var x2 = JsonReader.GetReader().Read<MySubClassThatKeepsAdditionalJsonFields>(jsonString);
            Assert.Equal(x1.myString, x2.myString);
            Log.d("x2.json=" + JsonWriter.GetWriter().Write(x2));
        }

        private class MyClass1 {

        }

        private class MySubClass1 : MyClass1 {
            public string myString;
            public string myString2;

            public Int64 myInt { get; set; }
        }

        private class MySubClassWithoutADefaultConstructor : MyClass1 {
            public string myString { get; set; }

            //The class has no default constructor but still can be instantiated by the JSON logic
            public MySubClassWithoutADefaultConstructor(string s) : base() { myString = s; }
        }

        private class MySubClassThatKeepsAdditionalJsonFields : MyClass1, HandleAdditionalJsonFields {
            public string myString;

            private Dictionary<string, object> additionalFieldsFromJsonThatAreMissingInClass;
            public Dictionary<string, object> GetMissingFields() {
                // additionalFieldsFromJsonThatAreMissingInClass = new Dictionary<string, object>();
                // additionalFieldsFromJsonThatAreMissingInClass.Add("test", "test");
                return additionalFieldsFromJsonThatAreMissingInClass;
            }
            public void SetMissingFields(Dictionary<string, object> additionalFieldsFromJsonThatAreMissingInClass) {
                this.additionalFieldsFromJsonThatAreMissingInClass = additionalFieldsFromJsonThatAreMissingInClass;
            }
        }

    }
}
