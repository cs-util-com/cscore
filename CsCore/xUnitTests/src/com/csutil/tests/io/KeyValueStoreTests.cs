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
        public async void TestAllIKeyValueStoreImplementations() {
            //await TestIKeyValueStoreImplementation(new LiteDbKeyValueStore());
            await TestIKeyValueStoreImplementation(new InMemoryKeyValueStore());
        }

        private static async Task TestIKeyValueStoreImplementation(IKeyValueStore store) {
            string myKey1 = "mykey1";
            var myValue1 = "myvalue1";
            string myKey2 = "mykey2";
            var myValue2 = "myvalue2";

            // test Set and Get of values:
            Assert.False(await store.ContainsKey(myKey1));
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

    }

}