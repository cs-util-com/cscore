using com.csutil.model.immutable;
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

    // The MonoBehaviour can be used for manual tests by attaching it to a scene
    class Ui3_DataStore : MonoBehaviour {
        private IEnumerator Start() { yield return new Ui3_DataStoreTest().ExampleUsage3(); }
    }

    class Ui3_DataStoreTest { // The automated unit test

        [UnityTest]
        public IEnumerator ExampleUsage3() {

            // Create an immutable datastore that will contain the data model in this example:
            var log = Middlewares.NewLoggingMiddleware<MyDataModel3>();
            IDataStore<MyDataModel3> store = new DataStore<MyDataModel3>(MainReducer, new MyDataModel3(), log);
            IoC.inject.SetSingleton(store);

            // Create a presenter that connectes the model with the view (the Unity UI):
            var currentUserPresenter = new MyUserUi3();
            // Set the target view by loading it from a prefab and setting the root GO:
            currentUserPresenter.targetView = ResourcesV2.LoadPrefab("MyUserUi1");
            // Connect the model changes with the presenter:
            currentUserPresenter.ListenToStoreUpdates(store, state => state.currentUser);

            // Dispatch a first setUser action to update the UI:
            store.Dispatch(new ActionSetNewUser() { newUser = new MyUser3("Carl", 99) });
            // Delay needed since the UI update simulates a delay too:
            yield return new WaitForSeconds(0.5f);
            // Check that the UI was automatically updated:
            Assert.AreEqual("Carl", currentUserPresenter.NameUi().text);
            Assert.AreEqual("99", currentUserPresenter.AgeUi().text);

            // Simulate that the user changed the model via the UI:
            store.Dispatch(new ActionUpdateUser() {
                target = store.GetState().currentUser,
                newValues = new MyUser3("Paul", 0)
            });
            // Delay needed since the UI update simulates a delay too:
            yield return new WaitForSeconds(0.1f);
            // Check that the UI was automatically updated:
            Assert.AreEqual("Paul", currentUserPresenter.NameUi().text);
            Assert.AreEqual("0", currentUserPresenter.AgeUi().text);

        }

        private class MyDataModel3 {
            public readonly MyUser3 currentUser;
            public MyDataModel3(MyUser3 currentUser = null) { this.currentUser = currentUser; }
        }

        private class MyUser3 {
            public readonly string name;
            public readonly int age;
            public MyUser3(string name, int age) { this.name = name; this.age = age; }
        }

        private class MyUserUi3 : Presenter<MyUser3> {

            public GameObject targetView { get; set; }
            private Dictionary<string, Link> links;

            public async Task OnLoad(MyUser3 model) {
                await TaskV2.Delay(10); // Simulate a 10ms delay in the UI update
                links = targetView.GetLinkMap();
                NameUi().text = model.name;
                AgeUi().text = "" + model.age;
                await links.Get<Button>("Save").SetOnClickAction(delegate { UpdateUser(model); });
            }

            public void UpdateUser(MyUser3 targetUser) {
                var newUserValues = new MyUser3(NameUi().text, int.Parse(AgeUi().text));
                var store = IoC.inject.Get<IDataStore<MyDataModel3>>(this);
                store.Dispatch(new ActionUpdateUser() { target = targetUser, newValues = newUserValues });
            }

            public InputField NameUi() { return links.Get<InputField>("Name"); }
            public InputField AgeUi() { return links.Get<InputField>("Age"); }

        }

        private class ActionSetNewUser { public MyUser3 newUser; }
        private class ActionUpdateUser { public MyUser3 target; public MyUser3 newValues; }

        private MyDataModel3 MainReducer(MyDataModel3 previousState, object action) {
            var changed = false;
            var newUser = previousState.currentUser.Mutate(action, ReduceUser, ref changed);
            if (changed) { return new MyDataModel3(newUser); }
            return previousState;
        }

        private MyUser3 ReduceUser(MyUser3 previousState, object action) {
            if (action is ActionSetNewUser a) { return a.newUser; }
            if (action is ActionUpdateUser a2) {
                Assert.AreEqual(a2.target, previousState);
                if (object.Equals(a2.target, previousState)) { return a2.newValues; }
            }
            return previousState;
        }

    }

}