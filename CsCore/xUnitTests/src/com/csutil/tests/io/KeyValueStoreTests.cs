using System.Threading.Tasks;
using com.csutil.io.keyvaluestore;
using Xunit;

namespace com.csutil.tests.io {
    public class KeyValueStoreTests {

        [Fact]
        public async void ExampleUsage1() {
            IKeyValueStore x = new InMemoryKeyValueStore();
            string myKey1 = "mykey1";
            string myValue1 = "myvalue1";
            await x.Set(myKey1, myValue1);
            Assert.Equal(myValue1, await x.Get<string>(myKey1, null));
        }

    }
}