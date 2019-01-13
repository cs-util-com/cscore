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
        public void TestMissingDefaultConstructor() {
            var x1 = new MySubClass1() { myString = "I am s1", myString2 = "I am s2" };
            var jsonString = JsonWriter.GetWriter().Write(x1 as MyClass1);
            var x2 = JsonReader.GetReader().Read<MySubClassWithoutADefaultConstructor>(jsonString);
            Assert.Equal(x1.myString, x2.myString);
        }

        [Fact]
        public void TestKeepingMissingFieldsInClass() {
            var x1 = new MySubClass1() { myString = "s1", myString2 = "s2", myComplexField = new MySubClass1() { myString2 = "s11" } };
            var x1JsonString = JsonWriter.GetWriter().Write(x1);
            var x2 = JsonReader.GetReader().Read<MySubClassThatKeepsAdditionalJsonFields>(x1JsonString);
            // myString2 and myComplexChildField are missing x2 as fields/porperties so the count of additionl json fields must be 2:
            Assert.Equal(2, x2.GetAdditionalJsonFields().Count);
            // The json will still contain the additional fields since they are attached again during serialization:
            var x2JsonString = JsonWriter.GetWriter().Write(x2);
            // Now parse it back to a MySubClass1 type:
            var x3 = JsonReader.GetReader().Read<MySubClass1>(x2JsonString);
            // Ensure that all fields from the original x1 are still there:
            Assert.Equal(x1.myString, x3.myString);
            Assert.Equal(x1.myString2, x3.myString2);
            Assert.Equal(x1.myComplexField.myString2, x3.myComplexField.myString2);
        }

        private class MyClass1 {

        }

        private class MySubClass1 : MyClass1 {
            public string myString;
            public string myString2;

            public MySubClass1 myComplexField { get; set; }
        }

        private class MySubClassWithoutADefaultConstructor : MyClass1 {
            public string myString { get; set; }

            //The class has no default constructor but still can be instantiated by the JSON logic
            public MySubClassWithoutADefaultConstructor(string s) : base() { myString = s; }
        }

        private class MySubClassThatKeepsAdditionalJsonFields : MyClass1, HandleAdditionalJsonFields {
            public string myString;

            private Dictionary<string, object> additionalFieldsFromJsonThatAreMissingInClass;
            public Dictionary<string, object> GetAdditionalJsonFields() {
                // additionalFieldsFromJsonThatAreMissingInClass = new Dictionary<string, object>();
                // additionalFieldsFromJsonThatAreMissingInClass.Add("test", "test");
                return additionalFieldsFromJsonThatAreMissingInClass;
            }
            public void SetAdditionalJsonFields(Dictionary<string, object> additionalFieldsFromJsonThatAreMissingInClass) {
                this.additionalFieldsFromJsonThatAreMissingInClass = additionalFieldsFromJsonThatAreMissingInClass;
            }
        }

    }
}
