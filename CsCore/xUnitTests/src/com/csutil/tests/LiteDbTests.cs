using System.Linq;
using LiteDB;
using Xunit;

namespace com.csutil.tests {

    public class LiteDbTests {

        public class Customer {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public string[] Phones { get; set; }
            public bool IsActive { get; set; }
        }

        [Fact]
        void ExampleUsage1() {
            // Open database (or create if doesn't exist)
            using (var db = new LiteDatabase(@"MyData.db")) {
                // Get customer collection
                var customers = db.GetCollection<Customer>("customers");

                // Create your new customer instance
                var customer = new Customer {
                    Name = "John Doe",
                    Phones = new string[] { "8000-0000", "9000-0000" },
                    Age = 39,
                    IsActive = true
                };

                // Create unique index in Name field
                customers.EnsureIndex(x => x.Name, true);

                // Insert new customer document (Id will be auto-incremented)
                if (customers.Exists(x => x.Name == customer.Name)) {
                    customers.Update(customer);
                } else {
                    customers.Insert(customer);
                }

                // Update a document inside a collection
                customer.Name = "Joana Doe";
                customers.Update(customer);

                // Use LINQ to query documents (with no index)
                var results = customers.Find(x => x.Age > 20);
                Assert.Single(results);
                Assert.Equal("Joana Doe", results.First().Name);
            }
        }

    }

}