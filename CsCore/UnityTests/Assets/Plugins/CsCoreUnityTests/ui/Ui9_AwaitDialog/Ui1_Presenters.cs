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

    public class Ui9_AwaitDialog : MonoBehaviour { IEnumerator Start() { yield return new Ui9_AwaitDialogTests().ExampleUsage(); } }

    public class Ui9_AwaitDialogTests {

        [UnityTest]
        public IEnumerator ExampleUsage() {

            MyDialog1Presenter userUiPresenter = new MyDialog1Presenter();
            userUiPresenter.targetView = ResourcesV2.LoadPrefab("MyDialog1");

            { // Load a first user into the UI by passing it through the presenter:
                var user1 = new MyDialog1Data() { userName = "Carl", userAge = 4 };
                yield return userUiPresenter.LoadModelIntoView(user1).AsCoroutine();
                Assert.AreEqual("Carl", userUiPresenter.NameInputField().text);
                Assert.AreEqual("4", userUiPresenter.AgeInputField().text);
            }

        }

        [System.Serializable]
        public class MyDialog1Data {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyDialog1Presenter : Presenter<MyDialog1Data> {

            public GameObject targetView { get; set; }
            Dictionary<string, Link> links;

            public async Task OnLoad(MyDialog1Data userToShow) {
                await TaskV2.Delay(5); // Simulate a delay
                links = targetView.GetLinkMap();
                NameInputField().text = userToShow.userName;
                AgeInputField().text = "" + userToShow.userAge;
                links.Get<Button>("Save").SetOnClickAction(delegate { SaveViewIntoModel(userToShow); });
            }

            private void SaveViewIntoModel(MyDialog1Data userToShow) {
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
