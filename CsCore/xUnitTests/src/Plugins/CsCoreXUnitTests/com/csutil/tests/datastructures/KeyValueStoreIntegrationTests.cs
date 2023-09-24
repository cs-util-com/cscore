using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.integrationTests.keyvaluestore {
    
    public class KeyValueStoreIntegrationTests {

        public KeyValueStoreIntegrationTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }
        
        [Fact]
        public async Task TestGoogleSheetsKeyValueStore() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = await IoC.inject.GetAppSecrets().GetSecret("GoogleSheetsV4Key");
            // See https://docs.google.com/spreadsheets/d/13R9y6lnUMgRPC0PinJ23tACC6Flgogxa7h7SVaaLhT0
            var sheetId = "13R9y6lnUMgRPC0PinJ23tACC6Flgogxa7h7SVaaLhT0";
            var sheetName = "UpdateEntriesV1"; // Has to match the sheet name

            var refreshDelayInMs = 1000;
            // The cache is where the data from the sheet will be locally stored in:
            var cache = new InMemoryKeyValueStore(); // Could also persist to disc here
            var store = new GoogleSheetsKeyValueStore(cache, apiKey, sheetId, sheetName, refreshDelayInMs);

            var download1 = store.dowloadOnlineDataDebounced();
            var download2 = store.dowloadOnlineDataDebounced();
            var download3 = store.dowloadOnlineDataDebounced();

            // After the refresh delay redownload was allowed again:
            await TaskV2.Delay(refreshDelayInMs * 3);

            Assert.True(await download1); // first trigger downloaded the data
            Assert.NotEmpty(await store.GetAllKeys());
            // Triggering it instant a second time will not download the data again:
            Assert.False(await download2); // Second trigger was skipped
            Assert.True(await download3);

            var entry1 = await store.Get<MySheetEntry>("1", null);
            Assert.NotNull(entry1);
            Assert.Equal("a", entry1.myString1);
            Assert.Equal(new List<int>() { 1, 2, 3, 4 }, entry1.myArray1);
            Assert.Equal("b", entry1.myObj1.a);
            Assert.Equal(5, entry1.myInt1);
            Assert.Equal(1.4, entry1.myDouble1);

            var entry2 = await store.Get<MySheetEntry>("2", null);
            Assert.NotNull(entry2);
            Assert.Null(entry2.myString1);
            Assert.Empty(entry2.myArray1);
            Assert.Null(entry2.myObj1);
            Assert.Equal(0, entry2.myInt1);
            Assert.Equal(0, entry2.myDouble1);

        }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        // Fields must be in sync with first row in the source Google Sheet, see names in:
        // https://docs.google.com/spreadsheets/d/13R9y6lnUMgRPC0PinJ23tACC6Flgogxa7h7SVaaLhT0
        private class MySheetEntry {
            public string myString1 { get; set; }
            public List<int> myArray1;
            public MyObj myObj1;
            public int myInt1;
            public double myDouble1;
            public class MyObj {
                public string a;
            }
        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }
    
}