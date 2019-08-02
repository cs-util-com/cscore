using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.tests.io {

    public class KeyValueStoreTests {

        public KeyValueStoreTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
            AssertV2.throwExeptionIfAssertionFails = true;
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
        public async void ExampleUsage2() {
            var storeFile = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChild("ExampleUsage2");
            storeFile.DeleteV2(); // Cleanup before tests if the test file exists
            string myKey1 = "test123";
            MyClass1 x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
            {   // Create a fast memory store and combine it with a LiteDB store that is persisted to disk:
                IKeyValueStore store = new InMemoryKeyValueStore().WithFallbackStore(new LiteDbKeyValueStore(storeFile));
                await store.Set(myKey1, x1);
                MyClass1 x2 = await store.Get<MyClass1>(myKey1, null);
                Assert.Equal(x1.myString1, x2.myString1);
                Assert.Equal(x1.myString2, x2.myString2);
            }
            { // Create a second store and check that the changes were persisted:
                IKeyValueStore store2 = new LiteDbKeyValueStore(storeFile);
                Assert.True(await store2.ContainsKey(myKey1));
                MyClass1 x2 = await store2.Get<MyClass1>(myKey1, null);
                Assert.Equal(x1.myString1, x2.myString1);
                Assert.Equal(x1.myString2, x2.myString2);
                await store2.Remove(myKey1);
                Assert.False(await store2.ContainsKey(myKey1));
            }
        }

        [Fact]
        public async void TestAllIKeyValueStoreImplementations() {
            var dbFile = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChild("TestAllIKeyValueStoreImplementations");
            dbFile.DeleteV2();
            await TestIKeyValueStoreImplementation(new InMemoryKeyValueStore());
            await TestIKeyValueStoreImplementation(new LiteDbKeyValueStore(dbFile));
        }

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
            await store.Remove(myKey2);
            Assert.False(await store.ContainsKey(myKey2));

            // Test RemoveAll:
            Assert.True(await store.ContainsKey(myKey1));
            await store.RemoveAll();
            Assert.False(await store.ContainsKey(myKey1));
        }

        [Fact]
        public async void TestExceptionCatching() {

            var kvstore = new InMemoryKeyValueStore();
            await kvstore.Set("1", 1);
            await Assert.ThrowsAsync<InvalidCastException>(() => kvstore.Get<string>("1", "myDefaultValue"));

            var kvstore2 = new ExceptionWrapperKeyValueStore(kvstore, new HashSet<Type>());
            string x = await kvstore2.Get<string>("1", "myDefaultValue");
            Assert.Equal("myDefaultValue", x);
            kvstore2.errorTypeBlackList.Add(typeof(InvalidCastException));
            await Assert.ThrowsAsync<InvalidCastException>(() => kvstore.Get<string>("1", "myDefaultValue"));

        }

    }

}