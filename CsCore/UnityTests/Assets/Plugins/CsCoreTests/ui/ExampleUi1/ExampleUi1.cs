using com.csutil.ui;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class ExampleUi1 {

        [UnityTest]
        public IEnumerator ExampleUsage1() {

            GameObject myUserUi1 = ResourcesV2.LoadPrefab("MyUserUi1");
            Presenter<MyUserModel> userUiPresenter = new MyUserUi();

            yield return new WaitForSeconds(4); // After 4 seconds load a user:

            {
                var user1 = new MyUserModel() { userName = "Carl", userAge = 4 };
                yield return userUiPresenter.Unload();
                yield return userUiPresenter.LoadModelIntoViewAsync(user1, myUserUi1);
            }

            yield return new WaitForSeconds(4); // Load another user into the UI:

            {
                var user2 = new MyUserModel() { userName = "Anna", userAge = 55 };
                yield return userUiPresenter.Unload();
                yield return userUiPresenter.LoadModelIntoViewAsync(user2, myUserUi1);
            }

            yield return new WaitForSeconds(20);
        }

        [System.Serializable]
        public class MyUserModel {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyUserUi : Presenter<MyUserModel> {

            public IEnumerator LoadModelIntoViewAsync(MyUserModel userToShow, GameObject userUi) {
                Dictionary<string, Link> links = userUi.GetLinkMap();

                links.Get<InputField>("Name").text = userToShow.userName;
                links.Get<InputField>("Age").text = "" + userToShow.userAge;

                links.Get<Button>("Save").SetOnClickAction(delegate {
                    userToShow.userName = links.Get<InputField>("Name").text;
                    userToShow.userAge = int.Parse(links.Get<InputField>("Age").text);
                    Log.d("User saved: " + userToShow);
                    ScreenStack.SwitchBackToLastScreen(userUi);
                });

                yield return null;
            }

            public IEnumerator Unload() {
                Log.d("Nothing to clean up");
                yield return null;
            }

        }


    }

}
