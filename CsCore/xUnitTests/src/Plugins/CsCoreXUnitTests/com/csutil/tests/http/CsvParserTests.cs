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
    
            JArray dataAsJson = new JArray();

            foreach (var row in parsedData)
            {
                JArray rowArray = new JArray(row.Select(JToken.FromObject));
                dataAsJson.Add(rowArray);
            }

            Assert.Equal(3, dataAsJson.Count); // 2 rows
            Assert.Equal(3, ((JArray)dataAsJson[0]).Count); // 3 columns

            Assert.Equal("Name", dataAsJson[0][0].ToString());
            Assert.Equal("Age", dataAsJson[0][1].ToString());
            Assert.Equal("Location", dataAsJson[0][2].ToString());

            Assert.Equal("Alice", dataAsJson[1][0].ToString());
            Assert.Equal("25", dataAsJson[1][1].ToString());
            Assert.Equal("New York", dataAsJson[1][2].ToString());
        }

    }
}