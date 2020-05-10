using com.csutil.model;
using com.csutil.model.mtvmtv;
using com.csutil.ui;
using com.csutil.ui.mtvmtv;
using Newtonsoft.Json;
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

            var vmtv = new ViewModelToView(mtvm, prefabFolder);
            var userView = await vmtv.ToView(viewModel);

            viewStack.ShowView(userView);

            Presenter<MyUserModel> p = new MyPresenter1();
            p.targetView = userView;

            MyUserModel model = new MyUserModel() {
                name = "Tom",
                email = "a@b.com",
                password = "12345678",
                age = 98,
                money = 0f,
                hasMoney = false,
                phoneNumber = "+1 234 5678 90",
                profilePic = new FileRef() { url = "https://picsum.photos/50/50" },
                bestFriend = new MyUserModel.UserContact() { name = "Bella" },
                description = "A normal person, nothing suspicious here..",
                homepage = "https://marvolo.uk"
            };
            await p.LoadModelIntoView(model);
            viewStack.SwitchBackToLastView(userView);
        }


        private class MyPresenter1 : Presenter<MyUserModel> {
            public GameObject targetView { get; set; }

            public async Task OnLoad(MyUserModel u) {

                var map = this.GetFieldViewMap();
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

                map.LinkViewToModel("bestFriend.name", u.bestFriend.name, newVal => u.bestFriend.name = newVal);
                map.LinkViewToModel("profilePic.url", u.profilePic.url, newVal => u.profilePic.url = newVal);

                await targetView.GetLinkMap().Get<Button>("ConfirmButton").SetOnClickAction(delegate { });
            }

        }

        private class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; }

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
