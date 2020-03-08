using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.json {

    public class ObjectAutoMapperTests {

        public ObjectAutoMapperTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        class User {
            public int id;
            public string myName;
            public string myEmail { get; set; }
            public string password; // should be dropped during mapping
            public User[] contacts;
        }

        class UserDTO {
            public int id;

            [JsonProperty("myName")]
            public string name;

            [JsonProperty("myEmail")]
            public string email { get; set; }

            [JsonProperty("contacts")]
            public User[] contactsMapper {
                set { // Example for a more complex mapping
                    contactEmails = value.Map(c => c.myEmail).ToList();
                }
            }

            public List<string> contactEmails { get; set; }
        }

        [Fact]
        public void ExampleUsage1() {
            var user1 = new User() {
                id = 6,
                myName = "Carl",
                myEmail = "carl@csutil.com",
                password = "123456",
                contacts = new User[] { new User() { myEmail = "contact1@ab.com" } }
            };

            // Map the user into its DTO:
            UserDTO user1Dto = Mapper.Map<UserDTO>(user1);
            Assert.Equal(user1.id, user1Dto.id);
            Assert.Equal(user1.myName, user1Dto.name);
            Assert.Equal(user1.myEmail, user1Dto.email);
            Assert.Equal(user1.contacts.First().myEmail, user1Dto.contactEmails.First());

            // Check that the password was not included in any form:
            string dtoJson = JsonWriter.GetWriter().Write(user1Dto);
            Assert.False(dtoJson.Contains(user1.password));
        }

    }

}