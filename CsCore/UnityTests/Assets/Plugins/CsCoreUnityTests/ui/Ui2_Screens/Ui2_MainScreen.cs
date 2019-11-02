using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.csutil;
using UnityEngine.UI;
using System;
using com.csutil.ui;
using System.Threading.Tasks;

namespace com.csutil.tests.ui {

    public class Ui2_MainScreen : MonoBehaviour {

        public Ui1_PresenterTests.MyUserModel currentUser = new Ui1_PresenterTests.MyUserModel() { userName = "Carl" };

        void Start() {
            var links = gameObject.GetLinkMap();
            links.Get<Button>("OptionsButton").SetOnClickAction(delegate {
                gameObject.GetViewStack().ShowView(gameObject, "ExampleUi2_OptionsScreen");
            });
            links.Get<Button>("UserDetailsButton").SetOnClickAction(ShowUserUi);
        }

        private async void ShowUserUi(GameObject buttonGo) {
            Ui1_PresenterTests.MyUserUi presenter = new Ui1_PresenterTests.MyUserUi();
            GameObject ui = gameObject.GetViewStack().ShowView(gameObject, "MyUserUi1");
            presenter.targetView = ui;
            await presenter.LoadModelIntoView(currentUser);

            var links = ui.GetLinkMap();
            AssertV2.AreEqual(currentUser.userName, links.Get<InputField>("Name").text, "userName");
        }

    }

}