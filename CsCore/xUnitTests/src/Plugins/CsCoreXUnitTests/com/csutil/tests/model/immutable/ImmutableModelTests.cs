using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace com.csutil.tests.model.immutable {

    public class ImmutableModelTests {

        public ImmutableModelTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> An immutable model should ideally have only readonly fields, which cant be set once the 
        /// object is constructed, so the json logic has to automatically use the correct constructor to assign the 
        /// field value based on the constructor parameter names </summary>
        [Fact]
        public void TestFromJsonForReadonlyFields() {
            MyClass1 x = new MyClass1(12, ImmutableList.Create(new string[] { "a", "b", "c" }));
            {
                var xAsJson = JsonWriter.GetWriter().Write(x);
                Assert.NotEmpty(xAsJson);

                // The fields are immutable so the json logic has to use the constructor to init them from the json: 
                var y = JsonReader.GetReader().Read<MyClass1>(xAsJson);
                Assert.Equal(x.myNumber, y.myNumber);
                AssertV3.AreEqualJson(x, y);
            }

            // The constructor json logic can also be mixed with normal field json logic:
            x.myMutableList = new List<string>() { "d", "e" };
            {
                var xAsJson = JsonWriter.GetWriter().Write(x);
                Assert.NotEmpty(xAsJson);
                var y = JsonReader.GetReader().Read<MyClass1>(xAsJson);

                // myMutableList is not required in the constructor but still correctly set by the json logic: 
                Assert.Equal(x.myMutableList, y.myMutableList);
                AssertV3.AreEqualJson(x, y);
            }
        }

        /// <summary> An immutable class with only one constructor and only readonly fields </summary>
        private class MyClass1 {

            /// <summary> Readonly fields should be preferred over properties with
            ///  private setters, see https://stackoverflow.com/a/7975677/165106 </summary>
            public readonly int myNumber;
            public readonly IImmutableList<string> myImmutableDict;
            public List<string> myMutableList { get; set; }

            /// <summary> The parameter names have to match the field/prop names for the json serialization to work </summary>
            public MyClass1(int myNumber, ImmutableList<string> myImmutableDict) {
                this.myNumber = myNumber;
                this.myImmutableDict = myImmutableDict;
            }

        }

    }

}