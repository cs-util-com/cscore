using System.Threading.Tasks;
using com.csutil.keyvaluestore;
using Xunit;

namespace com.csutil.tests.io {

    public class KeyValueStoreTests {

        public KeyValueStoreTests(Xunit.Abstractions.ITestOutputHelper logger) {
            logger.UseAsLoggingOutput();
            AssertV2.throwExeptionIfAssertionFails = true;
        }

        [Fact]
        public async void ExampleUsage1() {
            var dbFile = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChild("ExampleUsage1");
            dbFile.DeleteV2();
            IKeyValueStore s = new LiteDbKeyValueStore(dbFile);
            var key1 = "test123";
            var x1 = new MyClass1() { myString1 = "Abc", myString2 = "Abc2" };
            await s.Remove(key1);
            await s.Set(key1, x1);
            var x2 = await s.Get<MyClass1>(key1, null);
            Assert.Equal(x1.myString1, x2.myString1);
            Assert.Equal(x1.myString2, x2.myString2);
        }

        [Fact]
        public async void TestAllIKeyValueStoreImplementations() {
            var dbFile = EnvironmentV2.instance.GetOrAddTempFolder("KeyValueStoreTests").GetChild("TestAllIKeyValueStoreImplementations");
            dbFile.DeleteV2();
            await TestIKeyValueStoreImplementation(new LiteDbKeyValueStore(dbFile));
            await TestIKeyValueStoreImplementation(new InMemoryKeyValueStore());
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

        private class MyClass1 {
            public string myString1 { get; set; }
            public string myString2;
        }
    }

}