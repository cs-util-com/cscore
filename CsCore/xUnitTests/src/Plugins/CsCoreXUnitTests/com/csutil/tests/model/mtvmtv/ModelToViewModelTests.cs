using System;
using System.Collections.Generic;
using System.Linq;
using com.csutil.model;
using com.csutil.model.mtvmtv;
using Xunit;

namespace com.csutil.tests.model.mtvmtv {

    public class ModelToViewModelTests {

        public ModelToViewModelTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

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

            var mtvm = new ModelToViewModel();
            ViewModel vm = mtvm.ToViewModel("MyUserModel", user1);

            ViewModel profilePicVm = vm.fields["profilePic"].objVm;
            Assert.Equal("String", profilePicVm.fields["dir"].type);
            Assert.Equal("String", profilePicVm.fields["url"].type);

            Assert.Equal("Array", vm.fields["tags"].type);
            Assert.Equal("String", vm.fields["tags"].children.type);
            Assert.Null(vm.fields["tags"].children.entries);

            Assert.Equal("Array", vm.fields["contacts"].type);
            Assert.True(vm.fields["id"].readOnly.Value); // id has private setter
            Assert.True(vm.fields["contacts"].readOnly.Value); // contacts has only a getter
            Assert.Equal("Object", vm.fields["contacts"].children.type);

            ViewModel entryVm = vm.fields["contacts"].children.entries.First();
            Assert.Equal("" + typeof(MyUserModel.UserContact), entryVm.modelType);
            Assert.Null(entryVm.fields);

            Assert.Equal("" + typeof(MyUserModel.UserContact), vm.fields["bestFriend"].objVm.modelType);

            Assert.Equal("" + ContentType.Password, vm.fields["password"].contentType);

            ViewModel userVmInUserContactClass = vm.fields["bestFriend"].objVm.fields["user"].objVm;
            Assert.Equal("" + typeof(MyUserModel), userVmInUserContactClass.modelType);
            // The other fields of this ViewModel are null since it was already defined:
            Assert.Null(userVmInUserContactClass.fields);

            Log.d("viewModel: " + JsonWriter.AsPrettyString(vm));

        }

        [Fact]
        public void TestRecursiveModel() {

            var userContact1 = new MyUserModel.UserContact() {
                user = new MyUserModel() {
                    bestFriend = new MyUserModel.UserContact()
                }
            };
            var mtvm = new ModelToViewModel();
            ViewModel vm = mtvm.ToViewModel("UserContact", userContact1);
            var userVm = vm.fields["user"].objVm;
            var userContactVm = userVm.fields["bestFriend"].objVm;
            Assert.Equal("" + typeof(MyUserModel.UserContact), userContactVm.modelType);

        }

        [Fact]
        public void TestNullObjectResolved() {
            {
                var user = new MyUserModel.UserContact();
                var mtvm = new ModelToViewModel();
                ViewModel vm = mtvm.ToViewModel("UserContact", user);
                Assert.Null(user.user); // The model field is null
                Assert.NotNull(vm.fields["user"].objVm); // The viewmodel info is still defined
            }
            {
                var mtvm = new ModelToViewModel();
                ViewModel vm = mtvm.ToViewModel("UserContact", typeof(MyUserModel.UserContact));
                Assert.NotNull(vm.fields["user"].objVm); // The viewmodel info is still defined
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

            var mtvm = new ModelToViewModel();
            ViewModel vm = mtvm.ToViewModel("MyUserModel", json);

            Log.d(JsonWriter.AsPrettyString(vm));

            Assert.Equal("Age", vm.fields["user"].objVm.fields["age"].text.name);
            Assert.Equal("Integer", vm.fields["phoneNumbers"].children.type);


        }

        [Fact]
        public void TestNullable() {

            var user = new MyUserModel() { phoneNumber = 12345 };
            string json = JsonWriter.GetWriter().Write(user);

            ViewModel vm1 = new ModelToViewModel().ToViewModel("FromClassInstance", user);
            ViewModel vm2 = new ModelToViewModel().ToViewModel("FromJson", json);
            ViewModel vm3 = new ModelToViewModel().ToViewModel("FromClassType", typeof(MyUserModel));

            var n1 = vm1.fields["phoneNumber"];
            var n2 = vm2.fields["phoneNumber"];
            var n3 = vm3.fields["phoneNumber"];

            Assert.Equal(n1.type, n2.type);
            Assert.Equal(n2.type, n3.type);
            Assert.Equal(n1.text.name, n2.text.name);
            Assert.Equal(n2.text.name, n3.text.name);

        }

        [Fact]
        public void TestEnum() {

            ViewModel vm = new ModelToViewModel().ToViewModel("FromClassType", typeof(UserStats));
            var e1 = vm.fields["experience1"];
            var e2 = vm.fields["experience2"];
            var e3 = vm.fields["experience3"];
            var e4 = vm.fields["experience4"];
            Log.d(JsonWriter.AsPrettyString(vm));

            Assert.Equal(Enum.GetNames(typeof(UserStats.Experience)), e1.contentEnum);
            Assert.Equal(e1.contentEnum, e2.contentEnum);
            Assert.Equal(e2.contentEnum, e3.contentEnum);
            Assert.Equal(e3.contentEnum, e4.contentEnum);

        }

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
        private class MyUserModel {

            public string id { get; private set; }
            public string name;
            [Content(ContentType.Password, "A secure password")]
            public string password;
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

            public Experience experience4;

        }

#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}