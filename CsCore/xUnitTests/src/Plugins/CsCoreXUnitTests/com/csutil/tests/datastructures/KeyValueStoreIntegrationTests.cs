using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using com.csutil.tests.keyvaluestore;
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
        
        [Fact]
        public async Task TestStoreWithDelay() {
            // Simulates the DB on the server:
            var innerStore = new InMemoryKeyValueStore();
            // Simulates the connection to the server:
            var simulatedDelayStore = new MockDelayKeyValueStore(innerStore);
            // Handles connection problems to the server:
            var exWrapperStore = new ExceptionWrapperKeyValueStore(simulatedDelayStore);
            // Represents the local cache in case the server cant be reached:
            var outerStore = new InMemoryKeyValueStore().WithFallbackStore(exWrapperStore);

            var key1 = "key1";
            var value1 = "value1";
            var key2 = "key2";
            var value2 = "value2";

            {
                await outerStore.Set(key1, value1);
                Assert.Equal(value1, await outerStore.Get(key1, ""));
                Assert.Equal(value1, await innerStore.Get(key1, ""));
            }

            simulatedDelayStore.throwTimeoutError = true;
            var simulatedErrorCatched = false;
            exWrapperStore.onError = (Exception e) => { simulatedErrorCatched = true; };

            {
                await outerStore.Set(key2, value2); // This will cause a timeout error in the "delayed" store
                Assert.True(simulatedErrorCatched);
                Assert.Contains(key2, await outerStore.GetAllKeys()); // In the outer store the value was set
                Assert.False(await innerStore.ContainsKey(key2)); // The inner store never got the update
                Assert.False(await exWrapperStore.ContainsKey(key2)); // The exc. wrapper returns false if an error is thrown
                Assert.Null(await exWrapperStore.GetAllKeys()); // Will throw another error and return null
            }

            Log.d("innerStore " + innerStore.latestFallbackGetTimingInMs);
            Log.d("simulatedDelayStore " + simulatedDelayStore.latestFallbackGetTimingInMs);
            Log.d("exWrapperStore " + exWrapperStore.latestFallbackGetTimingInMs);
            Log.d("outerStore " + outerStore.latestFallbackGetTimingInMs);
            Assert.Equal(0, innerStore.latestFallbackGetTimingInMs);
            Assert.NotEqual(0, exWrapperStore.latestFallbackGetTimingInMs);
            Assert.NotEqual(0, outerStore.latestFallbackGetTimingInMs);

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