using System.Threading.Tasks;
using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {

    public class GoogleSheetsV4Tests {

        public GoogleSheetsV4Tests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var exampleApiKey = await IoC.inject.GetAppSecrets().GetSecret("GoogleSheetsV4Key");
            // E.g. https://docs.google.com/spreadsheets/d/1ixEZ7EkbPNrB5ZiIYL12kL25jKIIIvyz4nZKtqTDqWc
            var spreadsheetId = "1ixEZ7EkbPNrB5ZiIYL12kL25jKIIIvyz4nZKtqTDqWc";
            var sheetName = "MySheet 1"; // Has to match the sheet name
            Log.d("Will now load " + GoogleSheetsV4.GetShareLinkFor(spreadsheetId));
            Log.d("API request to " + GoogleSheetsV4.GetApiUrlFor(exampleApiKey, spreadsheetId, sheetName));
            var columns = await GoogleSheetsV4.GetSheet(exampleApiKey, spreadsheetId, sheetName);

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