using System;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.http.apis;
using com.csutil.system;
using Xunit;

namespace com.csutil.tests.http {

    public class GoogleSheetsV5Tests {

        public GoogleSheetsV5Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // E.g. https://docs.google.com/spreadsheets/d/1Hwu4ZtRR0iXD65Wuj_XyJxLw4PN8SE0sRgnBKeVoq3A
            // Use "File => Publish to the web" as a csv and use the csv url like this:
            var csvUrl = new Uri("https://docs.google.com/spreadsheets/d/e/2PACX-1vQhCWZHOifEU5liS9x_H6BA6BcpBHOHHc_28VC3oFM0xpkTMTFfn8D7MF_PUKQatyKxQFphTfSWXeDg/pub?gid=0&single=true&output=csv");
            var res = await GoogleSheetsV5.GetSheetObjects(csvUrl);

            var entry1 = res.First().Value;
            var entry1Json = JsonWriter.AsPrettyString(entry1);
            Log.d(entry1Json);
            var parsedAsNewsEntry = JsonReader.GetReader().Read<News>(entry1Json);
            Assert.Equal("Important Warning not to do xyz", parsedAsNewsEntry.title);
            
        }

    }

}