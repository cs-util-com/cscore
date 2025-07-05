using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using com.csutil.integrationTests.http;
using com.csutil.model;
using com.csutil.model.jsonschema;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.model.jsonschema {

    public class JsonSchemaTests {

        public JsonSchemaTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // A normal user model with a few example fields that users typically have:
            var user1 = new MyUserModel("" + GuidV2.NewGuid()) {
                name = "Tom",
                password = "12345678",
                age = 50,
                profilePic = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/128/128" },
                tags = new List<string>() { "tag1" }
            };

            var schemaGenerator = new ModelToJsonSchema();
            JsonSchema schema = schemaGenerator.ToJsonSchema("MyUserModel", user1);
            Log.d("schema: " + JsonWriter.AsPrettyString(schema));

            var profilePicVm = schema.properties["profilePic"];
            Assert.Equal("string", profilePicVm.properties["dir"].type);
            Assert.Equal("string", profilePicVm.properties["url"].type);

            Assert.Contains("id", schema.required);
            Assert.Contains("name", schema.required);
            Assert.Equal(2, schema.required.Count);

            Assert.Equal("array", schema.properties["tags"].type);
            Assert.Equal("string", schema.properties["tags"].items.First().type);
            Assert.Null(schema.properties["tags"].items.First().properties);

            Assert.Equal("array", schema.properties["contacts"].type);
            Assert.True(schema.properties["id"].readOnly.Value); // id has private setter
            Assert.True(schema.properties["contacts"].readOnly.Value); // contacts has only a getter

            Assert.Equal(30, schema.properties["name"].maxLength);

            Assert.Equal("object", schema.properties["contacts"].items.First().type);
            // Contacts schema already resolve as part of the bestFried field, so here no properties are included:
            Assert.Null(schema.properties["contacts"].items.First().properties);

            var entrySchema = schema.properties["contacts"].items.First();
            Assert.Equal("" + typeof(MyUserModel.UserContact), entrySchema.modelType);
            Assert.Null(entrySchema.properties);

            Assert.Equal("" + typeof(MyUserModel.UserContact), schema.properties["bestFriend"].modelType);

            Assert.Equal("" + ContentFormat.password, schema.properties["password"].format);

            var userSchemaInUserContactClass = schema.properties["bestFriend"].properties["user"];
            Assert.Equal("" + typeof(MyUserModel), userSchemaInUserContactClass.modelType);
            // The other fields of this json schema are null since it was already defined:
            Assert.Null(userSchemaInUserContactClass.properties);

            Assert.Equal(0, schema.properties["age"].minimum);
            Assert.Equal(130, schema.properties["age"].maximum);

        }

        [Fact]
        public void TestRecursiveModel() {

            var exampleUserContact1 = new MyUserModel.UserContact() {
                user = new MyUserModel() {
                    bestFriend = new MyUserModel.UserContact()
                }
            };
            var schemaGenerator = new ModelToJsonSchema();
            JsonSchema schema = schemaGenerator.ToJsonSchema("UserContact", exampleUserContact1);
            var bestFriendSchema = schema.properties["user"].properties["bestFriend"];
            Assert.Equal("" + typeof(MyUserModel.UserContact), bestFriendSchema.modelType);

        }

        [Fact]
        public void TestNullObjectResolved() {
            {
                var user = new MyUserModel.UserContact();
                var schemaGenerator = new ModelToJsonSchema();
                JsonSchema schema = schemaGenerator.ToJsonSchema("UserContact", user);
                Assert.Null(user.user); // The model field is null
                Assert.NotEmpty(schema.properties["user"].properties); // The schema info is still defined
            }
            {
                var schemaGenerator = new ModelToJsonSchema();
                var schema = schemaGenerator.ToJsonSchema("UserContact", typeof(MyUserModel.UserContact));
                Assert.NotEmpty(schema.properties["user"].properties); // The schema info is still defined
            }
        }

        [Fact]
        public void TestJsonToJsonSchema() {

            string json = JsonWriter.GetWriter().Write(new MyUserModel.UserContact() {
                phoneNumbers = new int[1] { 123 },
                user = new MyUserModel() {
                    name = "Tom",
                    age = 99,
                    phoneNumber = 12345
                }
            });
            Log.d("json=" + json);

            var schemaGenerator = new ModelToJsonSchema();
            var schema = schemaGenerator.ToJsonSchema("MyUserModel", json);

            Log.d(JsonWriter.AsPrettyString(schema));

            Assert.Equal("Age", schema.properties["user"].properties["age"].title);
            Assert.Equal("integer", schema.properties["phoneNumbers"].items.First().type);

        }

        [Fact]
        public void TestNullable() {

            var user = new MyUserModel() { phoneNumber = 12345 };
            string json = JsonWriter.GetWriter().Write(user);

            JsonSchema schema1 = new ModelToJsonSchema().ToJsonSchema("FromClassInstance", user);
            JsonSchema schema2 = new ModelToJsonSchema().ToJsonSchema("FromJson", json);
            JsonSchema schema3 = new ModelToJsonSchema().ToJsonSchema("FromClassType", typeof(MyUserModel));

            var phoneNumber1 = schema1.properties["phoneNumber"];
            var phoneNumber2 = schema2.properties["phoneNumber"];
            var phoneNumber3 = schema3.properties["phoneNumber"];

            Assert.Equal(phoneNumber1.type, phoneNumber2.type);
            Assert.Equal(phoneNumber2.type, phoneNumber3.type);
            Assert.Equal(phoneNumber1.title, phoneNumber2.title);
            Assert.Equal(phoneNumber2.title, phoneNumber3.title);

        }

        [Fact]
        public void TestEnum() {

            var schema = new ModelToJsonSchema().ToJsonSchema("FromClassType", typeof(UserStats));
            var e1 = schema.properties["experience1"];
            var e2 = schema.properties["experience2"];
            var e3 = schema.properties["experience3"];
            var e4 = schema.properties["experience4"];
            var e5 = schema.properties["experience5"];
            Log.d(JsonWriter.AsPrettyString(schema));

            Assert.Equal(Enum.GetNames(typeof(UserStats.Experience)), e1.contentEnum);
            Assert.Equal(e1.contentEnum, e2.contentEnum);
            Assert.Equal(e2.contentEnum, e3.contentEnum);
            Assert.Equal(e3.contentEnum, e4.contentEnum);
            Assert.Equal(e4.contentEnum, e5.contentEnum);

        }

        /// <summary>
        /// The following test uses the validator of System.ComponentModel to validate the model / object instance
        /// fields have values that are within the defined limits (enforced via the annotations on the model fields)
        /// </summary>
        [Fact]
        public void TestModelValidation() {

            var user = new MyUserModel() {
                name = "Tom",
                age = 50,
                money = 123.45f,
                profilePic = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/128/128" },
                bestFriend = new MyUserModel.UserContact() {
                    user = new MyUserModel() { name = "Jerry", age = 30 }
                },
                phoneNumber = 1234567890
            };

            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(user, new ValidationContext(user), validationResults, true);
            Assert.True(isValid, "User model should be valid, but was not: " + JsonWriter.GetWriter().Write(validationResults));
            Assert.Empty(validationResults); // No validation errors should be present
            
            // Now test with an invalid user model:
            {
                var invalidUser1 = new MyUserModel() {
                    name = "A very long name that exceeds the maximum length of 30 characters",
                    age = 150, // Invalid age
                    money = -10.0f, // Invalid money value
                    profilePic = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/128/128" },
                    bestFriend = new MyUserModel.UserContact() {
                        user = new MyUserModel() { name = "Jerry", age = 30 }
                    },
                    phoneNumber = null // Valid since it is nullable
                };
                validationResults.Clear();
                isValid = Validator.TryValidateObject(invalidUser1, new ValidationContext(invalidUser1), validationResults, true);
                Assert.False(isValid, "User model should be invalid, but was valid: " + JsonWriter.GetWriter().Write(invalidUser1));
                Assert.NotEmpty(validationResults); // Validation errors should be present
            }
            {
                var invalidUser2 = new MyUserModel() {
                    name = "Tom",
                    age = 50,
                    money = 123.45f,
                    profilePic = new FileRef() { url = RestTests.IMG_PLACEHOLD_SERVICE_URL + "/128/128" },
                    bestFriend = new MyUserModel.UserContact() {
                        user = new MyUserModel() { name = "A very long name that exceeds the maximum length of 30 characters", age = 130 }
                    },
                    phoneNumber = 1234567890
                };
                validationResults.Clear();
                isValid = Validator.TryValidateObject(invalidUser2, new ValidationContext(invalidUser2), validationResults, true);
                Assert.False(isValid, "User model should be invalid, but was valid: " + JsonWriter.GetWriter().Write(invalidUser2));
                Assert.NotEmpty(validationResults); // Validation errors should be present
            }

        }


#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        private class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; private set; } = GuidV2.NewGuid().ToString();

            [System.ComponentModel.DataAnnotations.Required]
            [System.ComponentModel.DataAnnotations.StringLength(30)]
            public string name { get; set; }

            [Content(ContentFormat.password, "A secure password")]
            public string password { get; set; }

            [System.ComponentModel.DataAnnotations.Range(minimum: 0, maximum: 130)]
            public int age { get; set; }
            [System.ComponentModel.DataAnnotations.Range(minimum: 0, maximum: float.MaxValue)]
            public float money { get; set; }
            public FileRef profilePic { get; set; }
            [ValidateObject]
            public UserContact bestFriend { get; set; }
            [Regex(RegexTemplates.PHONE_NR)]
            [System.ComponentModel.Description("e.g. +1 234 5678 90")]
            public int? phoneNumber { get; set; }

            public MyUserModel(string id = null) { this.id = id == null ? "" + GuidV2.NewGuid() : id; }

            public List<string> tags { get; set; }
            public List<UserContact> contacts { get; } = new List<UserContact>();

            public class UserContact {
                [ValidateObject] public MyUserModel user;
                public int[] phoneNumbers { get; set; }
                public List<MyUserModel> enemies;
            }

        }

        private class FileRef : IFileRef {

            public string dir { get; set; }
            public string fileName { get; set; }
            [System.ComponentModel.DataAnnotations.Required]
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }

        }

        private class UserStats {

            public enum Experience { Beginner, Avg, Expert }

            [Enum("Level of experience 1", typeof(Experience))]
            public string experience1;

            [Enum("Level of experience 2", true, "Beginner", "Avg", "Expert")]
            public string experience2;

            public Experience experience3;

            public Experience? experience4;

            [Enum("Level of experience 5", "Beginner", "Avg", "Expert")]
            public int experience5;

        }

#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}