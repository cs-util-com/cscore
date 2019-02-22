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

    class ExampleUi1 {

        [UnityTest]
        public IEnumerator ExampleUsage1() {

            GameObject myUserUi1 = ResourcesV2.LoadPrefab("MyUserUi1");
            Presenter<MyUserModel> userUiPresenter = new MyUserUi();

            yield return new WaitForSeconds(4); // After 4 seconds load a user:

            {
                var user1 = new MyUserModel() { userName = "Carl", userAge = 4 };
                yield return userUiPresenter.Unload();
                yield return userUiPresenter.LoadModelIntoView(user1, myUserUi1);
            }

            yield return new WaitForSeconds(4); // Load another user into the UI:

            {
                var user2 = new MyUserModel() { userName = "Anna", userAge = 55 };
                yield return userUiPresenter.Unload();
                yield return userUiPresenter.LoadModelIntoView(user2, myUserUi1);
            }

            yield return new WaitForSeconds(20);
        }

        private class MyUserModel {
            public string userName;
            public int userAge;
            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        private class MyUserUi : Presenter<MyUserModel> {

            public IEnumerator LoadModelIntoView(MyUserModel userToShow, GameObject userUi) {
                Dictionary<string, Link> links = userUi.GetLinkMap();

                AssertV2.IsNotNull(userToShow, "userToShow");
                AssertV2.IsFalse(links.IsNullOrEmpty(), "Links map is emtpy");
                AssertV2.IsNotNull(links.Get<Text>("Name"), "Name Link");

                links.Get<Text>("Name").text = userToShow.userName;
                links.Get<Text>("Age").text = "" + userToShow.userAge;

                links.Get<Button>("Save").SetOnClickAction(delegate {
                    userToShow.userName = links.Get<Text>("Name").text;
                    userToShow.userAge = int.Parse(links.Get<Text>("Age").text);
                    Log.d("User saved: " + userToShow);
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
