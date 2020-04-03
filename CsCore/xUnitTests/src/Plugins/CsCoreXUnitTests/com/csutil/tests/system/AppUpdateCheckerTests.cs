using com.csutil.keyvaluestore;
using com.csutil.system;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.system {

    public class AppUpdateCheckerTests {

        public AppUpdateCheckerTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var exampleApiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/1qZjRA_uLsImX-VHpJ1nnrCIASmU20Tbaakf5Le5Wrs8
            var spreadsheetId = "1qZjRA_uLsImX-VHpJ1nnrCIASmU20Tbaakf5Le5Wrs8";
            var sheetName = "UpdateEntriesV2"; // Has to match the sheet name
            var cache = new InMemoryKeyValueStore();
            var store = new GoogleSheetsKeyValueStore(cache, exampleApiKey, spreadsheetId, sheetName);

            // Use the GoogleSheets store as the source for the update information:
            var updateChecker = new DefaultAppUpdateChecker(store, null);

            var entries = await updateChecker.DownloadAllUpdateEntries();
            Assert.Equal(5, entries.Count());

            // Use the EnvironmentV2.instance.systemInfo values to match against all entries:
            var matchingEntries = await updateChecker.DownloadMatchingUpdateEntries();
            if (matchingEntries.Count > 0) {
                Assert.Single(matchingEntries);
                var instructions = matchingEntries.First().GetUpdateInstructions();
                Assert.Equal("https://github.com/cs-util-com/cscore", instructions.url);
                Log.d("instructions: " + JsonWriter.AsPrettyString(instructions));
            } else {
                Log.e("Test cant be fully done on current system: "
                    + JsonWriter.AsPrettyString(EnvironmentV2.instance.systemInfo));
            }

        }

    }

}