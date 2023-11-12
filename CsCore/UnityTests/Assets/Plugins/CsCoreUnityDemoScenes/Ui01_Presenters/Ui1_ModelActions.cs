using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui1_ModelActions : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        private async Task RunTestTask() {
            MyUserUi userUiPresenter = new MyUserUi();
            userUiPresenter.targetView = gameObject.GetViewStack().GetLatestView();
            userUiPresenter.actions = new UserActions();
            await RunTestOnUi(userUiPresenter);
        }

        private static async Task RunTestOnUi(MyUserUi userUiPresenter) {
            { // Load a first user into the UI by passing it through the presenter:
                var user1 = new MyUserModel() { userName = "Carl", userAge = 4 };
                await userUiPresenter.LoadModelIntoView(user1);
                AssertV2.AreEqual("Carl", userUiPresenter.NameInputField().text);
                AssertV2.AreEqual("4", userUiPresenter.AgeInputField().text);
            }
            await TaskV2.Delay(1000); // Load another user into the UI:
            { // Example of loading a second user in a separate asyn method "LoadUser2": 
                var user2 = new MyUserModel() { userName = "Anna", userAge = 55 };
                await userUiPresenter.LoadModelIntoView(user2);
                AssertV2.AreEqual("Anna", userUiPresenter.NameInputField().text);
                AssertV2.AreEqual("55", userUiPresenter.AgeInputField().text); // The age of user 2
            }
        }

        [System.Serializable]
        public class MyUserModel {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyUserUi : PresenterWithActions<MyUserModel, UserActions> {

            public GameObject targetView { get; set; }
            public UserActions actions { get; set; }
            Dictionary<string, Link> links;

            public async Task OnLoad(MyUserModel userToShow) {
                await TaskV2.Delay(5); // Simulate a delay
                links = targetView.GetLinkMap();
                NameInputField().text = userToShow.userName;
                AgeInputField().text = "" + userToShow.userAge;
                await links.Get<Button>("Save").SetOnClickAction(delegate {
                    actions.SaveUser(name: NameInputField().text, age: int.Parse(AgeInputField().text));
                    targetView.GetViewStack().SwitchBackToLastView(targetView);
                });
            }

            public InputField AgeInputField() { return links.Get<InputField>("Age"); }
            public InputField NameInputField() { return links.Get<InputField>("Name"); }

        }

        /// <summary> The model actions are pure C# code that can be developed and tested without Unity </summary>
        public class UserActions : IModelActions<MyUserModel> {

            public MyUserModel Model { private get; set; }
            public void SaveUser(string name, int age) {
                Model.userName = name;
                Model.userAge = age;
                Log.d("User saved: " + Model);
            }

        }

    }

}