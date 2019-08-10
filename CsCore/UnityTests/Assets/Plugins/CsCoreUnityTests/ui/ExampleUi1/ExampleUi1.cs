using com.csutil.ui;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class ExampleUi1 {

        [UnityTest]
        public IEnumerator ExampleUsage1() {

            GameObject myUserUi1 = ResourcesV2.LoadPrefab("MyUserUi1");
            MyUserUi userUiPresenter = new MyUserUi();

            { // Load a first user into the UI by passing it through the presenter:
                var user1 = new MyUserModel() { userName = "Carl", userAge = 4 };
                yield return userUiPresenter.LoadModelIntoView(user1, myUserUi1).AsCoroutine();
                Assert.AreEqual("Carl", userUiPresenter.NameInputField().text);
                Assert.AreEqual("4", userUiPresenter.AgeInputField().text);
            }

            yield return new WaitForSeconds(0.5f); // Load another user into the UI:

            { // Example of loading a second user in a separate asyn method "LoadUser2": 
                yield return LoadUser2(userUiPresenter, myUserUi1).AsCoroutine();
                Assert.AreEqual("55", userUiPresenter.AgeInputField().text); // The age of user 2
            }

        }

        private async Task LoadUser2(MyUserUi userUiPresenter, GameObject myUserUi1) {
            var user2 = new MyUserModel() { userName = "Anna", userAge = 55 };
            await userUiPresenter.LoadModelIntoView(user2, myUserUi1);
            Assert.AreEqual("Anna", userUiPresenter.NameInputField().text);
        }

        [System.Serializable]
        public class MyUserModel {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyUserUi : Presenter<MyUserModel> {
            Dictionary<string, Link> links;

            public IEnumerator LoadModelIntoViewCoroutine(MyUserModel userToShow, GameObject userUi) {

                yield return new WaitForSeconds(0.02f); // Simulate a delay

                links = userUi.GetLinkMap();

                NameInputField().text = userToShow.userName;
                AgeInputField().text = "" + userToShow.userAge;

                links.Get<Button>("Save").SetOnClickAction(delegate {
                    userToShow.userName = NameInputField().text;
                    userToShow.userAge = int.Parse(AgeInputField().text);
                    Log.d("User saved: " + userToShow);
                    userUi.GetViewStack().SwitchBackToLastView(userUi);
                });

                yield return null;
            }

            public InputField AgeInputField() { return links.Get<InputField>("Age"); }
            public InputField NameInputField() { return links.Get<InputField>("Name"); }

            public IEnumerator Unload() {
                if (links != null) { links.Clear(); }
                links = null;
                yield return null;
            }

        }

    }

}
