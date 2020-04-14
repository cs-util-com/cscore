using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.json {

    public class JsonPropertyAnnotationTests {

        public JsonPropertyAnnotationTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        class MyClass1 {
            [JsonProperty(Required = Required.Always)]
            public string myString;

            [JsonProperty(Required = Required.Always)]
            public string myString2 { get; set; }
        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

        [Fact]
        public void ExampleUsage1() {
            {
                string jsonString = "{'myString':'abc','myString2':'def'}";
                MyClass1 x = JsonReader.GetReader().Read<MyClass1>(jsonString);
                Assert.Equal("abc", x.myString);
                Assert.Equal("def", x.myString2);
            }
            Assert.Throws<JsonSerializationException>(() => {
                string jsonString = "{'myString':'abc'}";
                MyClass1 x = JsonReader.GetReader().Read<MyClass1>(jsonString);
            });
            Assert.Throws<JsonSerializationException>(() => {
                string jsonString = "{'myString2':'def'}";
                MyClass1 x = JsonReader.GetReader().Read<MyClass1>(jsonString);
            });
        }

    }

}