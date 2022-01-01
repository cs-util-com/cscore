using com.csutil.model.immutable;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Assertions;
using UnityEngine.UI;
using com.csutil;

namespace com.csutil.tests.ui {

    public class Ui26_MoreReduxWithUnityUiExamples : UnitTestMono {

        public override IEnumerator RunTest() { yield return RunTestTask().AsCoroutine(); }

        public async Task RunTestTask() {

            MyUser1 initialState = null; // Initially no user is logged in
            var store = new DataStore<MyUser1>(MyReducers1.ReduceUser, initialState, Middlewares.NewLoggingMiddleware<MyUser1>());

            var links = gameObject.GetLinkMap();

            var loginButton = links.Get<Button>("LoginBtn");
            var loginButtonWasClicked = loginButton.SetOnClickAction(async delegate {
                loginButton.gameObject.Destroy();
                await TaskV2.Run(async () => {
                    await TaskV2.Delay(1000);
                    store.Dispatch(new ActionLoginUser() { newLoggedInUser = new MyUser1("Karl") });
                }).LogOnError();
                Assert.IsNotNull(store.GetState());
            });

            // Register a listener that is attached to the UI button to demonstrate that its no longer triggered once the button is destroyed:
            var userNameChangedCounter = 0;
            var subStateListener = store.NewSubStateListenerForUnity(loginButton, user => user);
            subStateListener.AddStateChangeListener(x => x?.name, newName => {
                userNameChangedCounter++;
                Toast.Show("User name changed to " + newName);
                Assert.IsFalse(loginButton.IsDestroyed());
            }, triggerInstantToInit: false);

            var userInfoText1 = links.Get<InputField>("UserNameInput1");
            ConnectInputFieldUiToModel(userInfoText1, store);
            var userInfoText2 = links.Get<InputField>("UserNameInput2");
            ConnectInputFieldUiToModel(userInfoText2, store);

            var oldCounterValue = userNameChangedCounter;
            SimulateButtonClickOn("LoginBtn");
            await loginButtonWasClicked;
            Assert.IsTrue(loginButton.IsDestroyed());
            // Since the button was destroyed, the counter should not change anymore:
            Assert.AreEqual(oldCounterValue, userNameChangedCounter);

            Toast.Show("Changing user name from background thread...");
            await Task.Delay(2000);

            // When NewSubStateListener instead of NewSubStateListenerForUnity is used, the
            // event will arrive on the thread where it was dispatched:
            var wasCalledOnMainThread = true;
            store.NewSubStateListener(user => user).AddStateChangeListener(x => x.name, newName => {
                wasCalledOnMainThread = MainThread.isMainThread;
            }, triggerInstantToInit: false);

            await TaskV2.Run(async () => { store.Dispatch(new ChangeName() { newName = "Caaarl" }); });
            Assert.AreEqual("Caaarl", store.GetState().name);
            Assert.IsFalse(wasCalledOnMainThread);
        }

        private static void ConnectInputFieldUiToModel(InputField userInfoText, DataStore<MyUser1> store) {
            // When a new name is entered change it also in the model:
            userInfoText.SetOnValueChangedActionThrottled(newText => {
                store.Dispatch(new ChangeName() { newName = newText });
            });
            // When the name in the model changes update it also in the UI:
            userInfoText.SubscribeToStateChanges(store, x => x?.name, newName => {
                userInfoText.text = newName;
                // Since the unity version of subscribing to events is used, the events will always arrive on the main thread by default:
                Assert.IsTrue(MainThread.isMainThread);
            });
        }

        private class MyUser1 {
            public readonly string name;
            public MyUser1(string name) { this.name = name; }
        }

        private class ActionLoginUser { public MyUser1 newLoggedInUser; }
        private class ChangeName { public string newName; }

        private static class MyReducers1 { // The reducers to modify the immutable datamodel:

            // The most outer reducer is public to be passed into the store:
            public static MyUser1 ReduceUser(MyUser1 user, object action) {
                if (action is ActionLoginUser a1) { return a1.newLoggedInUser; }

                // Now the default pattern to call .Mutate on all fields and check via a changed flag if any
                // of the field were changed which would require a new instance of the object to be created:

                bool changed = false; // Will be set to true by the .Mutate method if any field changes:
                var newName = user?.name.Mutate(action, ReduceUserName, ref changed);
                if (changed) { return new MyUser1(newName); }
                return user; // None of the fields changed, old user can be returned
            }

            private static string ReduceUserName(string oldName, object action) {
                if (action is ChangeName a) { return a.newName; }
                return oldName;
            }

        }

    }

}