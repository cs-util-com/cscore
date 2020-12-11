using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui1_Presenters : UnitTestMono {

        public override IEnumerator RunTest() {

            MyUserUi userUiPresenter = new MyUserUi();
            userUiPresenter.targetView = gameObject.GetViewStack().GetLatestView();

            { // Load a first user into the UI by passing it through the presenter:
                var user1 = new MyUserModel() { userName = "Carl", userAge = 4 };
                yield return userUiPresenter.LoadModelIntoView(user1).AsCoroutine();
                AssertV2.AreEqual("Carl", userUiPresenter.NameInputField().text);
                AssertV2.AreEqual("4", userUiPresenter.AgeInputField().text);
            }

            yield return new WaitForSeconds(1f); // Load another user into the UI:

            { // Example of loading a second user in a separate asyn method "LoadUser2": 
                yield return LoadUser2(userUiPresenter).AsCoroutine();
                AssertV2.AreEqual("55", userUiPresenter.AgeInputField().text); // The age of user 2
            }

        }

        private async Task LoadUser2(MyUserUi userUiPresenter) {
            var user2 = new MyUserModel() { userName = "Anna", userAge = 55 };
            await userUiPresenter.LoadModelIntoView(user2);
            AssertV2.AreEqual("Anna", userUiPresenter.NameInputField().text);
        }

        [System.Serializable]
        public class MyUserModel {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyUserUi : Presenter<MyUserModel> {

            public GameObject targetView { get; set; }
            Dictionary<string, Link> links;

            public async Task OnLoad(MyUserModel userToShow) {
                await TaskV2.Delay(5); // Simulate a delay
                links = targetView.GetLinkMap();
                NameInputField().text = userToShow.userName;
                AgeInputField().text = "" + userToShow.userAge;
                var saveTask = links.Get<Button>("Save").SetOnClickAction(delegate { SaveViewIntoModel(userToShow); });
            }

            private void SaveViewIntoModel(MyUserModel userToShow) {
                userToShow.userName = NameInputField().text;
                userToShow.userAge = int.Parse(AgeInputField().text);
                Log.d("User saved: " + userToShow);
                targetView.GetViewStack().SwitchBackToLastView(targetView);
            }

            public InputField AgeInputField() { return links.Get<InputField>("Age"); }
            public InputField NameInputField() { return links.Get<InputField>("Name"); }

        }

    }

}
