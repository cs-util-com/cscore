using com.csutil.http.apis;
using System;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.http {

    // There is now a replacement, see GoogleSheetsV4Tests
    [Obsolete("The Google Sheets v3 API will be shut down on September 30, 2020, see https://developers.google.com/sheets/api/v3/data")]
    public class GoogleSheetsV3Tests {

        public GoogleSheetsV3Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // E.g. https://docs.google.com/spreadsheets/d/1ixEZ7EkbPNrB5ZiIYL12kL25jKIIIvyz4nZKtqTDqWc
            var testSheetId = "1ixEZ7EkbPNrB5ZiIYL12kL25jKIIIvyz4nZKtqTDqWc";
            Log.d("Will now load " + GoogleSheetsV3.GetShareLinkFor(testSheetId));
            Log.d("API request to " + GoogleSheetsV3.GetApiUrlFor(testSheetId));
            var columns = await GoogleSheetsV3.GetSheet(testSheetId);

            Assert.NotEmpty(columns);
            for (int i = 0; i < columns.Count; i++) {
                var column = columns[i];
                Assert.NotEmpty(column);
                for (int j = 0; j < column.Count; j++) {
                    var entry = column[j];
                    Log.d("entry " + i + "." + j + ": '" + entry + "'");
                }
            }

        }

    }

}