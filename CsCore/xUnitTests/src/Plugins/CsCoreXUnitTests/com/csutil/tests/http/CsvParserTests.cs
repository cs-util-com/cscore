using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var json = CsvParser.ConvertCsvToJArray(parsedData);
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

        private class Person {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Location { get; set; }
        }

        [Fact]
        public async Task ExampleUsage3() {
            // Made ConvertToJson Test based on ´ExampleUsage1´

            string sampleCsvData = "Name,Age,Location\nAlice,25,New York\nBob,30,San Francisco\n";
            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));
            List<List<string>> parsedData = CsvParser.ReadCsvStream(stream);

            var dataAsJson = CsvParser.ConvertCsvToJArray(parsedData);
            
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

            // Creating a MemoryStream from the CSV data
            using var stream = new MemoryStream(Encoding.Default.GetBytes(sampleCsvData));

            // Converting CSV data to a JSON array using CsvParser
            var dataAsJson = CsvParser.ReadCsvStreamAsJson(stream);
            

            // Creating a List of MyUsers
            List<MyUser> users = new List<MyUser>(dataAsJson.ToObject<List<MyUser>>());

            // Assertions
            Assert.NotNull(users); // Assert that the result is not null
            Assert.Equal(2, users.Count); // Assert that the correct number of MyUser objects were deserialized

            // Asserting details of the first user
            Assert.Equal("Alice", users[0].Name);
            Assert.Equal(25, users[0].Age);
            Assert.Equal("New York", users[0].Location);

            // Asserting details of the second user
            Assert.Equal("Bob", users[1].Name);
            Assert.Equal(30, users[1].Age);
            Assert.Equal("San Francisco", users[1].Location);

        }

        private class MyUser {
            public readonly string Name;
            public readonly int? Age;
            public readonly string Location;

            public MyUser(string name, int? age, string location) {
                Name = name;
                Age = age;
                Location = location;
            }
        }
    }
}