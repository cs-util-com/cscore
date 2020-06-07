using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model;
using com.csutil.model.mtvmtv;
using Newtonsoft.Json;
using Xunit;

namespace com.csutil.tests.model.mtvmtv {

    public class JsonSchemaTests {

        public JsonSchemaTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            // A normal user model with a few example fields that users typically have:
            var user1 = new MyUserModel("" + Guid.NewGuid()) {
                name = "Tom",
                password = "12345678",
                age = 50,
                profilePic = new FileRef() { url = "https://picsum.photos/128/128" },
                tags = new List<string>() { "tag1" }
            };

            var mtvm = new ModelToJsonSchema();
            var vm = mtvm.ToViewModel("MyUserModel", user1);
            Log.d("viewModel: " + JsonWriter.AsPrettyString(vm));

            var profilePicVm = vm.properties["profilePic"];
            Assert.Equal("string", profilePicVm.properties["dir"].type);
            Assert.Equal("string", profilePicVm.properties["url"].type);

            Assert.Contains("id", vm.required);
            Assert.Contains("name", vm.required);
            Assert.Equal(2, vm.required.Count);

            Assert.Equal("array", vm.properties["tags"].type);
            Assert.Equal("string", vm.properties["tags"].items.First().type);
            Assert.Null(vm.properties["tags"].items.First().properties);

            Assert.Equal("array", vm.properties["contacts"].type);
            Assert.True(vm.properties["id"].readOnly.Value); // id has private setter
            Assert.True(vm.properties["contacts"].readOnly.Value); // contacts has only a getter

            Assert.Equal("object", vm.properties["contacts"].items.First().type);
            // Contacts ViewModel already resolve as part of the bestFried field, so here no properties are included:
            Assert.Null(vm.properties["contacts"].items.First().properties);

            var entryVm = vm.properties["contacts"].items.First();
            Assert.Equal("" + typeof(MyUserModel.UserContact), entryVm.modelType);
            Assert.Null(entryVm.properties);

            Assert.Equal("" + typeof(MyUserModel.UserContact), vm.properties["bestFriend"].modelType);

            Assert.Equal("" + ContentType.Password, vm.properties["password"].contentType);

            var userVmInUserContactClass = vm.properties["bestFriend"].properties["user"];
            Assert.Equal("" + typeof(MyUserModel), userVmInUserContactClass.modelType);
            // The other fields of this ViewModel are null since it was already defined:
            Assert.Null(userVmInUserContactClass.properties);

            Assert.Equal(0, vm.properties["age"].minimum);
            Assert.Equal(130, vm.properties["age"].maximum);

        }

        [Fact]
        public void TestRecursiveModel() {

            var userContact1 = new MyUserModel.UserContact() {
                user = new MyUserModel() {
                    bestFriend = new MyUserModel.UserContact()
                }
            };
            var mtvm = new ModelToJsonSchema();
            var vm = mtvm.ToViewModel("UserContact", userContact1);
            var bestFriendVm = vm.properties["user"].properties["bestFriend"];
            Assert.Equal("" + typeof(MyUserModel.UserContact), bestFriendVm.modelType);

        }

        [Fact]
        public void TestNullObjectResolved() {
            {
                var user = new MyUserModel.UserContact();
                var mtvm = new ModelToJsonSchema();
                var vm = mtvm.ToViewModel("UserContact", user);
                Assert.Null(user.user); // The model field is null
                Assert.NotEmpty(vm.properties["user"].properties); // The viewmodel info is still defined
            }
            {
                var mtvm = new ModelToJsonSchema();
                var vm = mtvm.ToViewModel("UserContact", typeof(MyUserModel.UserContact));
                Assert.NotEmpty(vm.properties["user"].properties); // The viewmodel info is still defined
            }
        }

        [Fact]
        public void TestJsonToViewModel() {

            string json = JsonWriter.GetWriter().Write(new MyUserModel.UserContact() {
                phoneNumbers = new int[1] { 123 },
                user = new MyUserModel() {
                    name = "Tom",
                    age = 99,
                    phoneNumber = 12345
                }
            });
            Log.d("json=" + json);

            var mtvm = new ModelToJsonSchema();
            var vm = mtvm.ToViewModel("MyUserModel", json);

            Log.d(JsonWriter.AsPrettyString(vm));

            Assert.Equal("Age", vm.properties["user"].properties["age"].title);
            Assert.Equal("integer", vm.properties["phoneNumbers"].items.First().type);

        }

        [Fact]
        public void TestNullable() {

            var user = new MyUserModel() { phoneNumber = 12345 };
            string json = JsonWriter.GetWriter().Write(user);

            var vm1 = new ModelToJsonSchema().ToViewModel("FromClassInstance", user);
            var vm2 = new ModelToJsonSchema().ToViewModel("FromJson", json);
            var vm3 = new ModelToJsonSchema().ToViewModel("FromClassType", typeof(MyUserModel));

            var n1 = vm1.properties["phoneNumber"];
            var n2 = vm2.properties["phoneNumber"];
            var n3 = vm3.properties["phoneNumber"];

            Assert.Equal(n1.type, n2.type);
            Assert.Equal(n2.type, n3.type);
            Assert.Equal(n1.title, n2.title);
            Assert.Equal(n2.title, n3.title);

        }

        [Fact]
        public void TestEnum() {

            var vm = new ModelToJsonSchema().ToViewModel("FromClassType", typeof(UserStats));
            var e1 = vm.properties["experience1"];
            var e2 = vm.properties["experience2"];
            var e3 = vm.properties["experience3"];
            var e4 = vm.properties["experience4"];
            var e5 = vm.properties["experience5"];
            Log.d(JsonWriter.AsPrettyString(vm));

            Assert.Equal(Enum.GetNames(typeof(UserStats.Experience)), e1.contentEnum);
            Assert.Equal(e1.contentEnum, e2.contentEnum);
            Assert.Equal(e2.contentEnum, e3.contentEnum);
            Assert.Equal(e3.contentEnum, e4.contentEnum);
            Assert.Equal(e4.contentEnum, e5.contentEnum);

        }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        private class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; private set; } = Guid.NewGuid().ToString();

            [Required]
            public string name;

            [Content(ContentType.Password, "A secure password")]
            public string password;

            [MinMaxRange(min: 0, max: 130)]
            public int age;
            public float money;
            public FileRef profilePic;
            public UserContact bestFriend;
            [Regex(RegexTemplates.PHONE_NR)]
            [Description("e.g. +1 234 5678 90")]
            public int? phoneNumber;

            public MyUserModel(string id = null) { this.id = id == null ? "" + Guid.NewGuid() : id; }

            public List<string> tags { get; set; }
            public List<UserContact> contacts { get; } = new List<UserContact>();

            public class UserContact {
                public MyUserModel user;
                public int[] phoneNumbers { get; set; }
                public List<MyUserModel> enemies;
            }

        }

        private class FileRef : IFileRef {

            public string dir { get; set; }
            public string fileName { get; set; }
            [Required]
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }

        }

        private class UserStats {

            public enum Experience { Beginner, Avg, Expert }

            [Enum("Level of experience 1", typeof(Experience))]
            public string experience1;

            [Enum("Level of experience 2", "Beginner", "Avg", "Expert")]
            public string experience2;

            public Experience experience3;

            public Experience? experience4;

            [Enum("Level of experience 5", "Beginner", "Avg", "Expert")]
            public int experience5;

        }

#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}