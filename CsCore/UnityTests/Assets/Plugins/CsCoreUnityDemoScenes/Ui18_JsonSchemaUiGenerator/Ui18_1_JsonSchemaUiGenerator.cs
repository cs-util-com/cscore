using com.csutil.model;
using com.csutil.model.mtvmtv;
using com.csutil.ui;
using com.csutil.ui.mtvmtv;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests {

    public class Ui18_1_JsonSchemaUiGenerator : UnitTestMono {

        public static string prefabFolder = "mtvmtv1/";

        public override IEnumerator RunTest() {
            yield return GenerateAndShowViewFor(gameObject.GetViewStack()).AsCoroutine();
        }

        private static async Task GenerateAndShowViewFor(ViewStack viewStack) {

            { // Testing arrays / lists:
                var mtvm = new ModelToViewModel();
                JsonSchema viewModel = JsonReader.GetReader().Read<JsonSchema>(SomeJsonSchemaExamples.jsonSchema2);
                await LoadJsonModelIntoGeneratedJsonSchemaView(viewStack, mtvm, viewModel, SomeJsonSchemaExamples.json2);
            }

            { // This time load the viewModel from an external JSON schema:
                var mtvm = new ModelToViewModel();
                JsonSchema viewModel = JsonReader.GetReader().Read<JsonSchema>(SomeJsonSchemaExamples.jsonSchema1);
                await LoadJsonModelIntoGeneratedJsonSchemaView(viewStack, mtvm, viewModel, SomeJsonSchemaExamples.json1);
            }

            {
                var mtvm = new ModelToViewModel();
                var userModelType = typeof(MyUserModel);
                var viewModel = mtvm.ToViewModel("" + userModelType, userModelType);
                Log.d(JsonWriter.AsPrettyString(viewModel));
                await LoadModelIntoGeneratedView(viewStack, mtvm, viewModel);
            }

        }

        private static async Task LoadModelIntoGeneratedView(ViewStack viewStack, ModelToViewModel mtvm, JsonSchema viewModel) {
            MyUserModel model = NewExampleUserInstance();

            { // First an example to connect the model to a generated view via a manual presenter "MyManualPresenter1":
                var vmtv = NewViewModelToView(mtvm);
                GameObject generatedView = await vmtv.ToView(viewModel);
                viewStack.ShowView(generatedView);

                var presenter = new MyManualPresenter1();
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                await presenter.LoadModelIntoView(model);
                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(model));

                viewStack.SwitchBackToLastView(generatedView);

            }
            { // The second option is to use a generic JObjectPresenter to connect the model to the generated view:
                var vmtv = NewViewModelToView(mtvm);
                GameObject generatedView = await vmtv.ToView(viewModel);
                viewStack.ShowView(generatedView);

                var presenter = new JObjectPresenter(vmtv);
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                MyUserModel changedModel = await presenter.LoadViaJsonIntoView(model, VmtvContainerUtil.ChangesSavedViaConfirmButton(generatedView));
                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(changedModel));

                viewStack.SwitchBackToLastView(generatedView);
                var changedFields = MergeJson.GetDiff(model, changedModel);
                Log.d("Fields changed: " + changedFields?.ToPrettyString());
            }
        }

        private static async Task LoadJsonModelIntoGeneratedJsonSchemaView(ViewStack viewStack, ModelToViewModel mtvm, JsonSchema viewModel, string jsonModel) {
            JObject model = JsonReader.GetReader().Read<JObject>(jsonModel);

            {
                ViewModelToView vmtv = NewViewModelToView(mtvm);
                GameObject generatedView = await vmtv.ToView(viewModel);

                viewStack.ShowView(generatedView);

                var presenter = new JObjectPresenter(vmtv);
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + model.ToPrettyString());
                var changedModel = await presenter.LoadModelIntoView(model.DeepClone() as JObject);
                await VmtvContainerUtil.ChangesSavedViaConfirmButton(generatedView);
                Log.d("Model AFTER changes: " + changedModel.ToPrettyString());

                viewStack.SwitchBackToLastView(generatedView);
                var changedFields = MergeJson.GetDiff(model, changedModel);
                Log.d("Fields changed: " + changedFields?.ToPrettyString());
            }
        }

        private static ViewModelToView NewViewModelToView(ModelToViewModel mtvm) {
            return new ViewModelToView(mtvm, prefabFolder) { rootContainerPrefab = ViewModelToView.CONTAINER2 };
        }

        private class MyManualPresenter1 : Presenter<MyUserModel> {
            public GameObject targetView { get; set; }

            public async Task OnLoad(MyUserModel u) {

                var map = targetView.GetFieldViewMap();
                map.LinkViewToModel("id", u.id);
                map.LinkViewToModel("name", u.name, newVal => u.name = newVal);
                map.LinkViewToModel("lastName", u.lastName, newVal => u.lastName = newVal);
                map.LinkViewToModel("email", u.email, newVal => u.email = newVal);
                map.LinkViewToModel("password", u.password, newVal => u.password = newVal);
                map.LinkViewToModel("age", "" + u.age, newVal => u.age = int.Parse(newVal));
                map.LinkViewToModel("money", "" + u.money, newVal => u.money = float.Parse(newVal));
                map.LinkViewToModel("phoneNumber", u.phoneNumber, newVal => u.phoneNumber = newVal);
                map.LinkViewToModel("description", u.description, newVal => u.description = newVal);
                map.LinkViewToModel("homepage", u.homepage, newVal => u.homepage = newVal);
                map.LinkViewToModel("hasMoney", u.hasMoney, newVal => u.hasMoney = newVal);
                map.LinkViewToModel("exp1", u.exp1, newVal => u.exp1 = newVal);
                map.LinkViewToModel("exp2", "" + u.exp2, newVal => u.exp2 = MyUserModel.Experience.Avg.TryParse(newVal));

                map.LinkViewToModel("bestFriend.name", u.bestFriend.name, newVal => u.bestFriend.name = newVal);
                map.LinkViewToModel("profilePic.url", u.profilePic.url, newVal => u.profilePic.url = newVal);

                await VmtvContainerUtil.ChangesSavedViaConfirmButton(targetView);
            }

        }

        internal static MyUserModel NewExampleUserInstance() {
            return new MyUserModel() {
                name = "Tom",
                email = "a@b.com",
                password = "12345678",
                age = 98,
                money = 0f,
                hasMoney = false,
                phoneNumber = "+1 234 5678 90",
                profilePic = new FileRef() { url = "https://picsum.photos/50/50" },
                bestFriend = new MyUserModel.UserContact() {
                    name = "Bella",
                    user = new MyUserModel() {
                        name = "Bella",
                        email = "b@cd.e"
                    }
                },
                description = "A normal person, nothing suspicious here..",
                homepage = "https://marvolo.uk"
            };
        }

        internal class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; private set; } = Guid.NewGuid().ToString();

            [Regex(2, 30)]
            [Content(ContentType.Name, "e.g. Tom")]
            public string name;

            [Required]
            [Content(ContentType.Name, "e.g. Riddle")]
            public string lastName;

            [Content(ContentType.Email, "e.g. tom@email.com")]
            [Regex(RegexTemplates.EMAIL_ADDRESS)]
            public string email;

            [Content(ContentType.Password, "Lenght >= 6 & has A-Z a-z 0-9 ?!..")]
            [Regex(6, RegexTemplates.HAS_UPPERCASE, RegexTemplates.HAS_LOWERCASE, RegexTemplates.HAS_NUMBER, RegexTemplates.HAS_SPECIAL_CHAR)]
            public string password;

            [Description("e.g. 99")]
            [MinMaxRange(0,130)]
            public int? age;

            public float money;

            public enum Experience { Beginner, Avg, Expert }
            [Enum("Level of experience 1", typeof(Experience), additionalItems = true)]
            public string exp1;

            [Description("Level of experience 2")]
            public Experience exp2;

            [Description("Checked if there is any money")]
            public bool hasMoney;

            [Regex(RegexTemplates.PHONE_NR)]
            [Description("e.g. +1 234 5678 90")]
            public string phoneNumber;

            public FileRef profilePic;

            public UserContact bestFriend;

            [Content(ContentType.Essay, "A detailed description")]
            public string description;

            [Regex(RegexTemplates.URL)]
            public string homepage;

            //public List<string> tags { get; set; }

            //public List<UserContact> contacts { get; } = new List<UserContact>();

#pragma warning disable 0649 // Variable is never assigned to, and will always have its default value
            public class UserContact {

                [Content(ContentType.Name, "e.g. Barbara")]
                public string name;
                public MyUserModel user;

                //public int[] phoneNumbers { get; set; }

            }

        }

        internal class FileRef : IFileRef {

            [Regex(RegexTemplates.URL)]
            public string url { get; set; }

            public string dir { get; set; }
            public string fileName { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }

        }
#pragma warning restore 0649 // Variable is never assigned to, and will always have its default value

    }

}
