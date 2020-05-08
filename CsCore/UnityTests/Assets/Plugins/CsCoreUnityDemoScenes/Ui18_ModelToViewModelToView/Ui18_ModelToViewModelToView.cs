using com.csutil.model;
using com.csutil.model.mtvmtv;
using com.csutil.ui;
using com.csutil.ui.mtvmtv;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests {

    public class Ui18_ModelToViewModelToView : UnitTestMono {

        public static string prefabFolder = "mtvmtv1/";

        public override IEnumerator RunTest() {
            yield return GenerateAndShowViewFor(gameObject.GetViewStack()).AsCoroutine();
        }

        private static async Task GenerateAndShowViewFor(ViewStack viewStack) {
            var userView = await GenerateViewFor(typeof(MyUserModel), prefabFolder);
            viewStack.ShowView(userView);
            //LoadModelIntoGeneratedView(userView);
        }

        private static void LoadModelIntoGeneratedView(GameObject userView) {
            // TODO load model into generated view:
            Presenter<MyUserModel> p = new MyUserPresenter();
            p.targetView = userView;

            MyUserModel model = new MyUserModel() { name = "Tom" };
            p.LoadModelIntoView(model);
        }

        private static async Task<GameObject> GenerateViewFor(Type t, string prefabFolder) {
            var mtvm = new ModelToViewModel();
            ViewModel viewModel = mtvm.ToViewModel("" + t, t);
            Log.d(JsonWriter.AsPrettyString(viewModel));
            var vmtv = new ViewModelToView(mtvm, prefabFolder);
            return await vmtv.ToView(viewModel);
        }

        private class MyUserPresenter : Presenter<MyUserModel> {
            public GameObject targetView { get; set; }

            public Task OnLoad(MyUserModel model) {
                throw new NotImplementedException();
            }

        }

        private class MyUserModel {

            [JsonProperty(Required = Required.Always)]
            public string id { get; }

            [Content(ContentType.Name, "e.g. Tom Riddle")]
            public string name;

            [Content(ContentType.Email, "e.g. tom@email.com")]
            [Regex(RegexTemplates.EMAIL_ADDRESS)]
            public string email;

            [Content(ContentType.Password, "Minimum 50 characters")]
            public string password;

            [Description("e.g. 99")]
            public int? age;

            public float money;

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

            public class UserContact {

                public MyUserModel user;

                //public int[] phoneNumbers { get; set; }

            }

        }

        private class FileRef : IFileRef {

            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }

        }

    }

}
