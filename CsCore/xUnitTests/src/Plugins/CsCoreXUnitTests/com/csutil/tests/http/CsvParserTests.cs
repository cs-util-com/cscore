using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using com.csutil.io;
using Newtonsoft.Json.Linq;
using Xunit;

namespace com.csutil.tests.http {

    public class CsvParserTests {

        public CsvParserTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage() {

            string sampleCsvData = "Name,Age,Location\nAlice,25,New York\nBob,30,San Francisco\n";

            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));
            List<List<string>> parsedData = CsvParser.ReadCsvStream(stream);

            Assert.Equal(3, parsedData.Count); // 3 rows (including header)
            Assert.Equal(3, parsedData[0].Count); // 3 columns

            Assert.Equal("Name", parsedData[0][0]);
            Assert.Equal("Age", parsedData[0][1]);
            Assert.Equal("Location", parsedData[0][2]);

            Assert.Equal("Alice", parsedData[1][0]);
            Assert.Equal("25", parsedData[1][1]);
            Assert.Equal("New York", parsedData[1][2]);

            var json = CsvParser.ConvertToJson(parsedData);
            Log.d(JsonWriter.AsPrettyString(json));

        }

        [Fact]
        public async Task ExampleUsage2() {

            string sampleCsvData = "Name,Age,Location\nAlice,25,New York\nBob,30,San Francisco\n";

            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));
            List<Person> parsedData = CsvParser.ReadCsvStreamAs<List<Person>>(stream);

            Assert.Equal(2, parsedData.Count);
            var alice = parsedData[0];
            Assert.Equal("Alice", alice.Name);
            Assert.Equal(25, alice.Age);
            Assert.Equal("New York", alice.Location);

        }

        [Fact]
        public async Task ExampleUsage3() {

            string sampleCsvData = "Name,Age,Location\nAlice,25,New York\nBob,30,San Francisco\n";

            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));
            var dataAsJson = CsvParser.ReadCsvStreamAs<JArray>(stream);

            Assert.Equal("Alice", dataAsJson[0]["Name"].ToString());
            Assert.Equal("25", dataAsJson[0]["Age"].ToString());
            Assert.Equal("New York", dataAsJson[0]["Location"].ToString());

            Assert.Equal("Bob", dataAsJson[1]["Name"].ToString());
            Assert.Equal("30", dataAsJson[1]["Age"].ToString());
            Assert.Equal("San Francisco", dataAsJson[1]["Location"].ToString());

        }

        [Fact]
        public async Task ExampleUsage4() {

            string sampleCsvData = "Name,Age,Location\nAlice,25,New York\nBob,30,San Francisco\n";

            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));
            List<Person> parsedUserList = CsvParser.ReadCsvStreamAs<List<Person>>(stream);

            Assert.NotNull(parsedUserList); // Assert that the result is not null
            Assert.Equal(2, parsedUserList.Count); // Assert that the correct number of MyUser objects were deserialized

            // Asserting details of the first user
            Assert.Equal("Alice", parsedUserList[0].Name);
            Assert.Equal(25, parsedUserList[0].Age);
            Assert.Equal("New York", parsedUserList[0].Location);

            // Asserting details of the second user
            Assert.Equal("Bob", parsedUserList[1].Name);
            Assert.Equal(30, parsedUserList[1].Age);
            Assert.Equal("San Francisco", parsedUserList[1].Location);

        }

        private class Person {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Location { get; set; }
        }

    }

}