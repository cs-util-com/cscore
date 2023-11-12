using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using com.csutil.json;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.tests.keyvaluestore {

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
            { // Create a fast memory store and combine it with a LiteDB store that is persisted to disk:
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
            await TestIKeyValueStoreImplementation(NewZipFileBasedKeyValueStore("TestAllIKeyValueStoreImplementations_ZipFileDB"));
        }

        private static FileBasedKeyValueStore NewFileBasedKeyValueStore(string storeFolderName) {
            var dbFolder = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChildDir(storeFolderName);
            dbFolder.DeleteV2();
            return new FileBasedKeyValueStore(dbFolder);
        }

        private static ZipFileBasedKeyValueStore NewZipFileBasedKeyValueStore(string storeFolderName) {
            var dbFolder = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChildDir(storeFolderName);
            dbFolder.DeleteV2();
            var zipFile = dbFolder.CreateV2().GetChild("MyZipFile.zip");
            return ZipFileBasedKeyValueStore.New(zipFile);
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
        public async Task TestReadOnlyLiteDbKeyValueStoreFromStream() {
            var f = EnvironmentV2.instance.GetOrAddTempFolder("TestReadOnlyLiteDbKeyValueStoreFromStream");
            f.DeleteV2();
            var dbFile = f.CreateV2().GetChild("LiteDb1");
            string myKey1 = "myKey1";
            string val1 = "e1";
            using (IKeyValueStore store = new LiteDbKeyValueStore(dbFile)) {
                await store.Set("myKey1", val1);
            }
            // Now test how a steam (even a read only stream) can be accessed the same way:
            var memStream = new MemoryStream();
            using (var readOnlyStream = dbFile.OpenForRead()) { readOnlyStream.CopyTo(memStream); }
            using (IKeyValueStore store = new LiteDbKeyValueStore(memStream)) {
                Assert.Equal(val1, await store.Get(myKey1, ""));
            }
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

        [Fact]
        public async Task TestTypesNotLost() {

            TypedJsonHelper.SetupTypedJsonWriterAndReaderSingletons(overrideExisting: true);
            {
                var ms = new InMemoryKeyValueStore();
                await TestTypesNotLostWith(ms, ms);
            }
            {
                var folder = EnvironmentV2.instance.GetNewInMemorySystem();
                await TestTypesNotLostWith(new FileBasedKeyValueStore(folder), new FileBasedKeyValueStore(folder));
            }
            
        }

        [Fact]
        public async Task ZipFileInstantPersisted() {

            var dbFolder = EnvironmentV2.instance.GetOrAddTempFolder("ZipKeyValueStoreTests").GetChildDir("ZipFileInstantPersisted");
            dbFolder.DeleteV2();
            var zipFile = dbFolder.CreateV2().GetChild("MyZipFile.zip");

            {
                using var kvStore = ZipFileBasedKeyValueStore.New(zipFile, 1);
                Assert.Equal(0, zipFile.GetFileSize());

                // 1 open change is allowed so setting the first value will not write to the zip file:
                await kvStore.Set("1 key", "1 value");
                Assert.Equal(0, zipFile.GetFileSize());

                // 2 open changes are above the limit so the zip file will be written to disk:
                await kvStore.Set("2 key", "2 value");
                Assert.True(zipFile.GetFileSize() > 0, $"zipFile.GetFileSize()={zipFile.GetFileSize()}");
            }

            { // Do a lot of parallel operations on the store to test the flushing does not cause other operations happening at the same time to fail:
                using var kvStore = ZipFileBasedKeyValueStore.New(zipFile, 100);
                var tasks = new List<Task>();
                for (int i = 0; i < 10000; i++) {
                    string s = "" + i;
                    tasks.Add(TaskV2.Run(async () => {
                        await kvStore.Set(s, s);
                        var res = await kvStore.Get(s, "<value not found>");
                        if (res != s) {
                            throw new InvalidDataException("res=" + res + " != " + s);
                        }
                        await kvStore.Remove(s);
                    }));
                }
                await Task.WhenAll(tasks);
            }

        }
        
        private static async Task TestTypesNotLostWith(IKeyValueStore s1, IKeyValueStore s2) {
            var x = new MySheetEntry() { myInt1 = 123 };
            await s1.Set("1", x);
            var res = await s2.Get<object>("1", null);
            Assert.IsType<MySheetEntry>(res);
            Assert.Equal(x.myInt1, ((MySheetEntry)res).myInt1);
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