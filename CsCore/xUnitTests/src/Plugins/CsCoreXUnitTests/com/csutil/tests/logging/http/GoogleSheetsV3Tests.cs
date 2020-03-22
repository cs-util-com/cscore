using com.csutil.http.apis;
using Newtonsoft.Json;
using System;
using System.Linq;
using Xunit;

namespace com.csutil.tests.http {

    [Obsolete("The Google Sheets v3 API will be shut down on September 30, 2020, see https://developers.google.com/sheets/api/v3/data")]
    public class GoogleSheetsV3Tests {

        public GoogleSheetsV3Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async void ExampleUsage1() {

            var testSheetId = "1ixEZ7EkbPNrB5ZiIYL12kL25jKIIIvyz4nZKtqTDqWc";
            Log.d("Will now load " + GoogleSheetsV3.GetShareLinkFor(testSheetId));
            Log.d("API request to " + GoogleSheetsV3.GetApiUrlFor(testSheetId));
            var sheets = await GoogleSheetsV3.GetSheets(testSheetId);
            Assert.NotEmpty(sheets);
            for (int i = 0; i < sheets.Count; i++) {
                var column = sheets[i].ToList();
                Assert.NotEmpty(column);
                for (int j = 0; j < column.Count; j++) {
                    var entry = column[j];
                    Log.d("entry " + i + "." + j + ": '" + entry + "'");
                }
            }

            // If a document is not published you will get an access error:
            await Assert.ThrowsAsync<JsonReaderException>(async () => {
                await GoogleSheetsV3.GetSheets("17b-O4HNCIwDC4ebwKZdxYi8zplqK9tJh4Zl8tDPM5Wc");
            });

        }

    }
}