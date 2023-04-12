using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using com.csutil.io;
using Xunit;

namespace com.csutil.tests.http {

    [Collection("Sequential")] // Will execute tests in here sequentially
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

    }

}