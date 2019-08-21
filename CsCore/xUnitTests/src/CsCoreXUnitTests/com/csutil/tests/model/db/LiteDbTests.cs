using System;
using System.Linq;
using com.csutil.model;
using LiteDB;
using Xunit;

namespace com.csutil.tests.model {

    public class LiteDbTests {

        private class User : HasId {
            public string id { get; set; }
            public string name { get; set; }
            public int age { get; set; }
            public string GetId() { return id; }
        }

        [Fact]
        void ExampleUsage1() {

            string testId = Guid.NewGuid().ToString();

            var dbFile = EnvironmentV2.instance.GetOrAddTempFolder("tests.io.db").GetChild("TestDB_" + testId);

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(dbFile.FullPath())) {

                var users = db.GetCollection<User>("users");

                var user1 = new User {
                    id = testId,
                    name = "John Doe",
                    age = 39
                };

                // Create unique index in Name field
                // https://github.com/mbdavid/LiteDB/wiki/Indexes#ensureindex
                users.EnsureIndex(x => x.name, true);

                users.Insert(user1);

                user1.name = "Joana Doe";
                users.Update(user1);
                Assert.Equal("Joana Doe", users.FindById(testId).name);

                user1.name = "Joana Doe 2";
                users.Upsert(user1); // insert or update if already found
                Assert.Equal("Joana Doe 2", users.FindById(testId).name);

                // Use LINQ to query documents (with no index)
                var queryResults = users.Find(x => x.age > 20);
                Assert.Single(queryResults);
                Assert.Equal("Joana Doe 2", queryResults.First().name);

            }
            Assert.True(dbFile.IsNotNullAndExists());
            dbFile.DeleteV2(); // cleanup after the test
        }

    }

}