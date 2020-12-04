using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.tests.keyvaluestore {

    [Collection("Sequential")] // Will execute tests in here sequentially
    public class KeyValueStoreTests {

        public KeyValueStoreTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
        }

        private class MyClass1 {
            public string myString1 { get; set; }
            public string myString2;
        }

        [Fact]
        public void ExampleUsage1() {
            IKeyValueStore store = new InMemoryKeyValueStore();
            string myKey1 = "myKey1";
            MyClass1 x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
            store.Set(myKey1, x1);
            MyClass1 x2 = store.Get<MyClass1>(myKey1, defaultValue: null).Result;
            Assert.Equal(x1.myString1, x2.myString1);
            Assert.Equal(x1.myString2, x2.myString2);
        }

        [Fact]
        public async Task ExampleUsage2() {
            var storeDir = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChildDir("ExampleUsage2Dir");
            storeDir.DeleteV2(); // Cleanup before tests if the test file exists
            string myKey1 = "test123";
            MyClass1 x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
            {   // Create a fast memory store and combine it with a LiteDB store that is persisted to disk:
                IKeyValueStore store = new InMemoryKeyValueStore().WithFallbackStore(new FileBasedKeyValueStore(storeDir));
                await store.Set(myKey1, x1);
                MyClass1 x2 = await store.Get<MyClass1>(myKey1, null);
                Assert.Equal(x1.myString1, x2.myString1);
                Assert.Equal(x1.myString2, x2.myString2);
            }
            { // Create a second store and check that the changes were persisted:
                IKeyValueStore store2 = new FileBasedKeyValueStore(storeDir);
                Assert.True(await store2.ContainsKey(myKey1));
                MyClass1 x2 = await store2.Get<MyClass1>(myKey1, null);
                Assert.Equal(x1.myString1, x2.myString1);
                Assert.Equal(x1.myString2, x2.myString2);
                Assert.True(await store2.Remove(myKey1));
                Assert.False(await store2.ContainsKey(myKey1));
            }
        }

        [Fact]
        public async Task ExampleUsage3() {

            // Simulate the DB on the server
            var simulatedDb = new InMemoryKeyValueStore();
            // Simulate the connection (with a delay) to the server:
            var simulatedRemoteConnection = new MockDelayKeyValueStore(simulatedDb);

            // The connection to the server is wrapped by a automatic retry for failing requests:
            var requestRetry = new RetryKeyValueStore(simulatedRemoteConnection, maxNrOfRetries: 5);
            // Any errors in the inner layers like connection errors, DB errors are catched by default:
            var errorHandler = new ExceptionWrapperKeyValueStore(requestRetry);
            // The outer store is a local in memory cache and the main point of contact:
            var outerStore = new InMemoryKeyValueStore().WithFallbackStore(errorHandler);

            var key1 = "key1";
            var value1 = "value1";
            var fallback1 = "fallback1";
            await outerStore.Set(key1, value1);
            Assert.Equal(value1, await outerStore.Get(key1, fallback1));
            Assert.Equal(value1, await simulatedDb.Get(key1, fallback1));

            // Simmulate connection problems to the remote DB:
            simulatedRemoteConnection.throwTimeoutError = true;

            var key2 = "key2";
            var value2 = "value2";
            var fallback2 = "fallback2";
            // Awaiting a set will take some time since there will be 5 retries:
            await outerStore.Set(key2, value2);
            // The outer store has the set value cached:
            Assert.Equal(value2, await outerStore.Get(key2, fallback2));
            // But the request never reached the simulated DB:
            Assert.False(await simulatedDb.ContainsKey(key2));

        }

        [Fact]
        public async Task TestAllIKeyValueStoreImplementations() {
            await TestIKeyValueStoreImplementation(new InMemoryKeyValueStore());
            await TestIKeyValueStoreImplementation(new ExceptionWrapperKeyValueStore(new InMemoryKeyValueStore()));
            await TestIKeyValueStoreImplementation(new MockDelayKeyValueStore(new InMemoryKeyValueStore()));
            await TestIKeyValueStoreImplementation(NewFileBasedKeyValueStore("TestAllIKeyValueStoreImplementations_FileDB"));
            await TestIKeyValueStoreImplementation(NewLiteDbStoreForTesting("TestAllIKeyValueStoreImplementations_LiteDB"));
            await TestIKeyValueStoreImplementation(new DualStore(new InMemoryKeyValueStore(), new InMemoryKeyValueStore()));
        }


        private static FileBasedKeyValueStore NewFileBasedKeyValueStore(string storeFolderName) {
            var dbFolder = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChildDir(storeFolderName);
            dbFolder.DeleteV2();
            return new FileBasedKeyValueStore(dbFolder);
        }

        private static LiteDbKeyValueStore NewLiteDbStoreForTesting(string storeFileName) {
            var dbFile = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChild(storeFileName);
            dbFile.DeleteV2();
            return new LiteDbKeyValueStore(dbFile);
        }

        /// <summary> Runs typical requests on the passed store </summary>
        private static async Task TestIKeyValueStoreImplementation(IKeyValueStore store) {
            string myKey1 = "myKey1";
            var myValue1 = "myValue1";
            string myKey2 = "myKey2";
            var myValue2 = "myValue2";
            var myFallbackValue1 = "myFallbackValue1";

            // test Set and Get of values:
            Assert.False(await store.ContainsKey(myKey1));
            Assert.Equal(myFallbackValue1, await store.Get(myKey1, myFallbackValue1));
            await store.Set(myKey1, myValue1);
            Assert.Equal(myValue1, await store.Get<string>(myKey1, null));
            Assert.True(await store.ContainsKey(myKey1));

            // Test replacing values:
            var oldVal = await store.Set(myKey1, myValue2);
            Assert.Equal(myValue1, oldVal);
            Assert.Equal(myValue2, await store.Get<string>(myKey1, null));

            // Test add and remove of a second key:
            Assert.False(await store.ContainsKey(myKey2));
            await store.Set(myKey2, myValue2);
            Assert.True(await store.ContainsKey(myKey2));

            var keys = await store.GetAllKeys();
            Assert.Equal(2, keys.Count());

            Assert.True(await store.Remove(myKey2));
            Assert.False(await store.ContainsKey(myKey2));

            // Test RemoveAll:
            Assert.True(await store.ContainsKey(myKey1));
            await store.RemoveAll();
            Assert.False(await store.ContainsKey(myKey1));
        }

        [Fact]
        public async Task TestExceptionCatching() {
            var myKey1 = "key1";
            int myValue1 = 1;
            string myDefaultString = "myDefaultValue";

            var innerStore = new InMemoryKeyValueStore();
            var exHandlerStore = new ExceptionWrapperKeyValueStore(innerStore, new HashSet<Type>());

            await innerStore.Set(myKey1, myValue1);
            // Cause an InvalidCastException:
            await Assert.ThrowsAsync<InvalidCastException>(() => innerStore.Get<string>(myKey1, myDefaultString));
            // Cause an InvalidCastException which is then catched and instead the default is returned:
            string x = await exHandlerStore.Get<string>(myKey1, myDefaultString);
            Assert.Equal(myDefaultString, x);

            // Add the InvalidCastException to the list of errors that should not be ignored:
            exHandlerStore.errorTypeBlackList.Add(typeof(InvalidCastException));
            // Now the same Get request passes the InvalidCastException on:
            await Assert.ThrowsAsync<InvalidCastException>(() => exHandlerStore.Get<string>(myKey1, myDefaultString));
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

        [Fact]
        public async Task TestCachingValuesFromFallbackStores() {

            var key1 = "key1";
            var value1 = "value1";
            var fallback1 = "fallback1";

            var s1 = new InMemoryKeyValueStore();
            var s2 = NewFileBasedKeyValueStore("TestCachingValuesFromFallbackStoresDir").WithFallbackStore(s1);
            var s3 = new InMemoryKeyValueStore().WithFallbackStore(s2);

            await s1.Set(key1, value1);
            // s3 will ask s2 which will ask s1 so the value will be returned correctly:
            Assert.Equal(value1, await s3.Get(key1, fallback1));

            // Now the value should also be cached in the other stores, so s1 is not needed anymore:
            s2.fallbackStore = null;
            s3.fallbackStore = null;
            Assert.Equal(value1, await s2.Get(key1, fallback1));
            Assert.Equal(value1, await s3.Get(key1, fallback1));

        }

        [Fact]
        public async Task TestReplaceAndRemove() {

            var key1 = "key1";
            var value1 = "value1";
            var fallback1 = "fallback1";

            var s1 = new InMemoryKeyValueStore();
            var s2 = NewFileBasedKeyValueStore("TestReplaceAndRemoveDir").WithFallbackStore(s1);
            var s3 = new InMemoryKeyValueStore().WithFallbackStore(s2);

            await s1.Set(key1, value1);
            // Test that replace with same value via s3 returns the old value (which is also value1):
            Assert.Equal(value1, await s3.Set(key1, value1));
            // s3 will ask s2 which will ask s1 so the value will be returned correctly:
            Assert.Equal(value1, await s3.Get(key1, fallback1));
            Assert.Single(await s3.GetAllKeys());

            Assert.True(await s3.Remove(key1));
            // Setting it again will return null since it was removed from all stores: 
            Assert.Null(await s3.Set(key1, value1));
            Assert.True(await s1.Remove(key1)); // Remove it only from s1
            Assert.Equal(value1, await s3.Get(key1, fallback1)); // Still cached in s3 and s2
            // s1 had the key already removed, so the combined remove result will be false:
            Assert.False(await s3.Remove(key1));

        }

        [Fact]
        public async Task TestDelayStoreWithExponentialBackoffRetry() {

            // Simulates the DB on the server:
            var innerStore = new InMemoryKeyValueStore();
            // Simulates the connection to the server:
            var simulatedRemoteConnection = new MockDelayKeyValueStore(innerStore);
            var requestRetry = new RetryKeyValueStore(simulatedRemoteConnection, maxNrOfRetries: 5);
            var outerStore = new InMemoryKeyValueStore().WithFallbackStore(requestRetry);

            var key1 = "key1";
            var value1 = "value1";
            var key2 = "key2";
            var value2 = "value2";
            var fallback2 = "fallback2";

            {
                await outerStore.Set(key1, value1);
                Assert.Equal(value1, await outerStore.Get(key1, ""));
                Assert.Equal(value1, await innerStore.Get(key1, ""));
            }

            // Now simulate that the remote DB/server never can be reached:
            simulatedRemoteConnection.throwTimeoutError = true;

            {
                var timeoutErrorCounter = 0;
                // In the retry listen to any error if the wrapped store:
                requestRetry.onError = (e) => {
                    Assert.IsType<TimeoutException>(e); // thrown by the simulatedRemoteConnection
                    timeoutErrorCounter++;
                };

                var delayedSetTask = outerStore.Set(key2, value2);
                Assert.Equal(value2, await outerStore.Get(key2, fallback2)); // In the outer store the value was set
                Assert.False(await innerStore.ContainsKey(key2)); // The inner store never got the update

                // The delayedSetTask was canceled after 5 retries: 
                await Assert.ThrowsAsync<OperationCanceledException>(async () => await delayedSetTask);
                // There will be minimum 5 TimeoutException in the simulatedRemoteConnection:
                Assert.True(requestRetry.maxNrOfRetries <= timeoutErrorCounter);
            }

        }

        enum MyEnum1 { state1, state2 }

        [Fact]
        public void TestEnumSetAndGet1() {
            var jsonReader = TypedJsonHelper.NewTypedJsonReader();
            var jsonWriter = TypedJsonHelper.NewTypedJsonWriter();

            var x1 = new ValueWrapper() { value = MyEnum1.state2 };
            var json = jsonWriter.Write(x1);
            Log.d("json=" + json);
            var x2 = jsonReader.Read<ValueWrapper>(json);

            Assert.Equal(MyEnum1.state2, x2.GetValueAs<MyEnum1>());
        }

        [Fact]
        public async Task TestEnumSetAndGet2() {
            var f = EnvironmentV2.instance.GetOrAddTempFolder("TestEnumSetAndGet");
            f.DeleteV2();
            IKeyValueStore store = new FileBasedKeyValueStore(f);
            string myKey1 = "myKey1";
            MyEnum1 e1 = MyEnum1.state2;
            await store.Set(myKey1, e1);
            MyEnum1 e2 = await store.Get(myKey1, MyEnum1.state1);
            Assert.Equal(e1, e2);
        }

        [Fact]
        public async Task TestGoogleSheetsKeyValueStore() {

            // Get your key from https://console.developers.google.com/apis/credentials
            var apiKey = "AIzaSyCtcFQMgRIUHhSuXggm4BtXT4eZvUrBWN0";
            // See https://docs.google.com/spreadsheets/d/13R9y6lnUMgRPC0PinJ23tACC6Flgogxa7h7SVaaLhT0
            var sheetId = "13R9y6lnUMgRPC0PinJ23tACC6Flgogxa7h7SVaaLhT0";
            var sheetName = "UpdateEntriesV1"; // Has to match the sheet name

            var refreshDelayInMs = 1000;
            // The cache is where the data from the sheet will be locally stored in:
            var cache = new InMemoryKeyValueStore(); // Could also persist to disc here
            var store = new GoogleSheetsKeyValueStore(cache, apiKey, sheetId, sheetName, refreshDelayInMs);

            Assert.True(await store.dowloadOnlineDataDebounced());
            Assert.NotEmpty(store.latestRawSheetData);
            // Triggering it instant a second time will not download the data again:
            Assert.False(await store.dowloadOnlineDataDebounced());

            // Waiting the refresh delay will allow to redownload:
            await TaskV2.Delay(refreshDelayInMs * 2);
            Assert.True(await store.dowloadOnlineDataDebounced());

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
        public async Task TestDualStore() {

            var firstStore = new InMemoryKeyValueStore();
            var secondStor = new InMemoryKeyValueStore();

            var dualStore = new DualStore(firstStore, secondStor);

            var key1 = "key1";
            var key2 = "key2";
            var value1 = "v1";
            var fallb1 = "f1";

            Assert.False(await dualStore.ContainsKey(key1));
            Assert.Null(await dualStore.Set(key1, value1));
            Assert.True(await dualStore.ContainsKey(key1));
            Assert.True(await firstStore.ContainsKey(key1));
            Assert.False(await secondStor.ContainsKey(key1));

            Assert.Single(await dualStore.GetAllKeys());
            Assert.Single(await firstStore.GetAllKeys());
            Assert.Empty(await secondStor.GetAllKeys());

            Assert.Equal(value1, await dualStore.Get(key1, fallb1));
            Assert.Equal(value1, await firstStore.Get(key1, fallb1));
            Assert.Equal(fallb1, await secondStor.Get(key1, fallb1));
            Assert.Equal(fallb1, await dualStore.Get(key2, fallb1));

            Assert.True(await dualStore.Remove(key1));
            Assert.Equal(fallb1, await dualStore.Get(key1, fallb1));
            Assert.Equal(fallb1, await firstStore.Get(key1, fallb1));
            Assert.False(await dualStore.Remove(key2));

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
            public class MyObj { public string a; }
        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}