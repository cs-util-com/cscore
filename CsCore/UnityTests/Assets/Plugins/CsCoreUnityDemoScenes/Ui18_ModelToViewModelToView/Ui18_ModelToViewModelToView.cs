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

    public class Ui18_ModelToViewModelToView : UnitTestMono {

        public static string prefabFolder = "mtvmtv1/";

        public override IEnumerator RunTest() {
            yield return GenerateAndShowViewFor(gameObject.GetViewStack()).AsCoroutine();
        }

        private static async Task GenerateAndShowViewFor(ViewStack viewStack) {
            var t = typeof(MyUserModel);

            var mtvm = new ModelToViewModel();
            ViewModel viewModel = mtvm.ToViewModel("" + t, t);
            Log.d(JsonWriter.AsPrettyString(viewModel));

            await LoadModelIntoGeneratedView(viewStack, mtvm, viewModel);

        }

        private static async Task LoadModelIntoGeneratedView(ViewStack viewStack, ModelToViewModel mtvm, ViewModel viewModel) {
            MyUserModel model = NewExampleUserInstance();

            { // First an example to connect the model to a generated view via a manual presenter "MyManualPresenter1":
                GameObject generatedView = await GenerateViewFromViewModel(mtvm, viewModel);
                viewStack.ShowView(generatedView);

                var presenter = new MyManualPresenter1();
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                await presenter.LoadModelIntoView(model);
                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(model));

                viewStack.SwitchBackToLastView(generatedView);

            }
            { // The second option is to use a generic JObjectPresenter to connect the model to the generated view:
                GameObject generatedView = await GenerateViewFromViewModel(mtvm, viewModel);
                viewStack.ShowView(generatedView);

                var presenter = new JObjectPresenter();
                presenter.targetView = generatedView;

                Log.d("Model BEFORE changes: " + JsonWriter.AsPrettyString(model));
                MyUserModel changedModel = await presenter.LoadViaJsonIntoView(model, SetupConfirmButton(generatedView));
                Log.d("Model AFTER changes: " + JsonWriter.AsPrettyString(changedModel));

                viewStack.SwitchBackToLastView(generatedView);
                var changedFields = MergeJson.GetDiff(model, changedModel);
                Log.d("Fields changed: " + changedFields?.ToPrettyString());
            }
        }

        private static async Task SetupConfirmButton(GameObject targetView) {
            do {
                await ConfirmButtonClicked(targetView);
            } while (!RegexValidator.IsAllInputCurrentlyValid(targetView));
        }

        private static Task ConfirmButtonClicked(GameObject targetView) {
            return targetView.GetLinkMap().Get<Button>("ConfirmButton").SetOnClickAction(async delegate {
                Toast.Show("Saving..");
                await TaskV2.Delay(500); // Wait for potential pending throttled actions to update the model
            });
        }

        private static async Task<GameObject> GenerateViewFromViewModel(ModelToViewModel mtvm, ViewModel viewModel) {
            var vmtv = new ViewModelToView(mtvm, prefabFolder) { rootContainerPrefab = ViewModelToView.CONTAINER2 };
            return await vmtv.ToView(viewModel);
        }

        private class MyManualPresenter1 : Presenter<MyUserModel> {
            public GameObject targetView { get; set; }

            public async Task OnLoad(MyUserModel u) {

                var map = targetView.GetFieldViewMap();
                map.LinkViewToModel("id", u.id);
                map.LinkViewToModel("name", u.name, newVal => u.name = newVal);
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

                await SetupConfirmButton(targetView);
            }

        }

        private static MyUserModel NewExampleUserInstance() {
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

        private class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; private set; } = Guid.NewGuid().ToString();

            [Regex(2, 30)]
            [Content(ContentType.Name, "e.g. Tom Riddle")]
            public string name;

            [Content(ContentType.Email, "e.g. tom@email.com")]
            [Regex(RegexTemplates.EMAIL_ADDRESS)]
            public string email;

            [Content(ContentType.Password, "Lenght >= 6 & has A-Z a-z 0-9 ?!..")]
            [Regex(6, RegexTemplates.HAS_UPPERCASE, RegexTemplates.HAS_LOWERCASE, RegexTemplates.HAS_NUMBER, RegexTemplates.HAS_SPECIAL_CHAR)]
            public string password;

            [Description("e.g. 99")]
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

        private class FileRef : IFileRef {

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
