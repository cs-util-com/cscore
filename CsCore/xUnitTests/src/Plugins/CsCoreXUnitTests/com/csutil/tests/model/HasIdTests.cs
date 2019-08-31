using com.csutil.keyvaluestore;
using com.csutil.model;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.model {

    public class HasIdTests {

        private class MyUserClass1 : HasId {
            public string id;
            public string name;
            public string GetId() { return id; }
        }

        [Fact]
        public async Task TestHasIdDbConnection() {

            var userId1 = "123";
            MyUserClass1 user1 = new MyUserClass1() { id = userId1, name = "Some Name" };

            { // Inject the HasIdDbConnection globally:
                // If the store is not registered x.Save() will throw an injection exception:
                await Assert.ThrowsAsync<InjectionException>(async () => { await user1.SaveToDb(); });
                IoC.inject.SetSingleton(new HasIdDbConnection<MyUserClass1>(new InMemoryKeyValueStore()));
            }
            { // Now use the extension method x.Save() that will use the injected store internally:
                Assert.Null(await user1.SaveToDb());
                user1.name = "Some new name";
                Assert.Equal(user1, await user1.SaveToDb());
            }
            { // HasId also provides an extension method for Load that does the same:
                MyUserClass1 defaultValue = null;
                MyUserClass1 loadedUser1 = await defaultValue.LoadFromDb(userId1);
                Assert.Equal(user1.name, loadedUser1.name);
            }
            { // The injected DB can also be used directly without the extension methods:
                var injectedDB = IoC.inject.Get<HasIdDbConnection<MyUserClass1>>(this);
                // Loading the same item again from the DB:
                MyUserClass1 loadedUser1 = await injectedDB.Load(userId1, null);
                Assert.Equal(user1.name, loadedUser1.name);
                // The DB can also load all entries:
                var allEntries = await injectedDB.LoadAllEntries();
                Assert.Contains(user1, allEntries);
            }

        }

    }

}