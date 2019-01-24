using System;
using System.Collections.Generic;
using System.Diagnostics;
using com.csutil.json;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.json {

    public class JsonTests {

        private class MyClass1 {
        }

        private class MySubClass1 : MyClass1 {
            public string myString;
            public string myString2;
            public MySubClass1 myComplexField { get; set; }
        }

        private class MySubClass2_WithNoDefaultConstructor : MyClass1 {
            public string myString { get; set; }
            
            //The class has no default constructor but still can be instantiated by the JSON logic
            public MySubClass2_WithNoDefaultConstructor(string s) : base() { myString = s; }
        }

        [Fact]
        public void TestMissingDefaultConstructor() {
            var x1 = new MySubClass1() { myString = "I am s1", myString2 = "I am s2" };
            var jsonString = JsonWriter.GetWriter().Write(x1 as MyClass1);
            var x2 = JsonReader.GetReader().Read<MySubClass2_WithNoDefaultConstructor>(jsonString);
            Assert.Equal(x1.myString, x2.myString);
        }

    }

}
