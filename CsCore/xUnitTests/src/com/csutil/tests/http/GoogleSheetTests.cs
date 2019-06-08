using com.csutil.http.apis;
using Xunit;

namespace com.csutil.tests.http {
    public class GoogleSheetTests {

        [Fact]
        public async void ExampleUsage1() {
            string testSheetId = "1sK1YAgWuxoLWSdiXZzdXF25SUB113lmYntpXPITMwqw";
            var sheet = await GoogleSheets.GetSheet(testSheetId);
            Assert.NotEqual(0, sheet.Count);
            for (int i = 0; i < sheet.Count; i++) {
                var column = sheet[i];
                Assert.NotEqual(0, column.Count);
                for (int j = 0; j < column.Count; j++) {
                    var entry = column[j];
                    Log.d("entry " + i + "." + j + ": '" + entry + "'");
                }
            }
        }

    }
}