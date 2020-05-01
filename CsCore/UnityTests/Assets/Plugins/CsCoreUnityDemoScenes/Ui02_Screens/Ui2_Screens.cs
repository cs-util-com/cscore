using com.csutil.analytics;
using com.csutil.keyvaluestore;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui2_Screens : UnitTestMono {

        public override IEnumerator RunTest() {

            IoC.inject.SetSingleton<IPreferences>(PlayerPrefsStore.NewPreferencesUsingPlayerPrefs());
            AppFlow.AddAppFlowTracker(new AppFlowToLog().WithAllTrackingActive());

            var links = gameObject.GetLinkMap();
            links.Get<Button>("OptionsButton").SetOnClickAction(delegate {
                gameObject.GetViewStack().ShowView("ExampleUi2_OptionsScreen", gameObject);
            });
            links.Get<Button>("UserDetailsButton").SetOnClickAction(ShowUserUi);
            yield return null;

        }

        public Ui1_Presenters.MyUserModel currentUser = new Ui1_Presenters.MyUserModel() { userName = "Carl" };

        private async void ShowUserUi(GameObject buttonGo) {
            Ui1_Presenters.MyUserUi presenter = new Ui1_Presenters.MyUserUi();
            GameObject ui = gameObject.GetViewStack().ShowView("MyUserUi1", gameObject);
            presenter.targetView = ui;
            await presenter.LoadModelIntoView(currentUser);

            var links = ui.GetLinkMap();
            AssertV2.AreEqual(currentUser.userName, links.Get<InputField>("Name").text, "userName");
        }

    }

}