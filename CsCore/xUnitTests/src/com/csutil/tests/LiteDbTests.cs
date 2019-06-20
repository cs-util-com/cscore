using System;
using System.Linq;
using com.csutil.io.db;
using LiteDB;
using Xunit;

namespace com.csutil.tests.io.db {

    public class LiteDbTests {

        public class Customer : HasId {
            public string id { get; set; }
            public string name { get; set; }
            public int age { get; set; }
            public string GetId() { return id; }
        }

        [Fact]
        void ExampleUsage1() {

            var dbFile = EnvironmentV2.instance.GetAppDataFolder().GetChildDir("tests.io.db").GetChild("ExampleUsage1.db");
            dbFile.ParentDir().CreateV2();
            dbFile.DeleteV2(); // for the test ensure that the db file does not yet exist

            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(dbFile.FullPath())) {
                // Get customer collection
                var customers = db.GetCollection<Customer>("customers");

                string testId = Guid.NewGuid().ToString();

                // Create your new customer instance
                var customer = new Customer {
                    id = testId,
                    name = "John Doe",
                    age = 39
                };

                // Create unique index in Name field
                // https://github.com/mbdavid/LiteDB/wiki/Indexes#ensureindex
                customers.EnsureIndex(x => x.name, true);

                customers.Insert(customer);

                customer.name = "Joana Doe";
                customers.Update(customer);
                Assert.Equal("Joana Doe", customers.FindById(testId).name);

                customer.name = "Joana Doe 2";
                customers.Upsert(customer); // insert or update if already found
                Assert.Equal("Joana Doe 2", customers.FindById(testId).name);

                // Use LINQ to query documents (with no index)
                var results = customers.Find(x => x.age > 20);
                Assert.Single(results);
                Assert.Equal("Joana Doe 2", results.First().name);
            }
            Assert.True(dbFile.IsNotNullAndExists());
        }

    }

}