using com.csutil.logging;
using com.csutil.model.immutable;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.tests.ui16 {

    public class Ui16_InputFormExamples : UnitTestMono {

        public MyDataModel initialModel = new MyDataModel() { user = new MyUser() };

        public override IEnumerator RunTest() {
            LogConsole.RegisterForAllLogEvents(this);

            DataStore<MyDataModel> store = new DataStore<MyDataModel>(Reducers.MainReducer, initialModel);

            var map = gameObject.GetLinkMap();

            Task showForm1 = map.Get<Button>("ShowForm1").SetOnClickAction(async delegate {
                MyFormPresenter presenter = new MyFormPresenter();
                presenter.targetView = gameObject.GetViewStack().ShowView("Ui16_MyForm1");
                presenter.simulateUserInput = simulateUserInput;
                await presenter.LoadModelIntoView(store);
            });

            Task showForm2 = map.Get<Button>("ShowForm2").SetOnClickAction(async delegate {
                MyFormPresenter presenter = new MyFormPresenter();
                presenter.targetView = gameObject.GetViewStack().ShowView("Ui16_MyForm1");
                presenter.simulateUserInput = simulateUserInput;
                var fork = store.NewFork();
                await presenter.LoadModelIntoView(fork);
                fork.ApplyMutationsBackToOriginalStore();
                ShowFormCompletedDebugInfos(fork);
            });

            if (simulateUserInput) {
                SimulateButtonClickOn("ShowForm1");
                yield return showForm1.AsCoroutine();
                SimulateButtonClickOn("ShowForm2");
                yield return showForm2.AsCoroutine();
            }

        }

        private void ShowFormCompletedDebugInfos(ForkedStore<MyDataModel> fork) {
            Snackbar.Show($"The {fork.recordedActions.Count} changes to the user are now saved",
                "Show details", delegate {
                    LogConsole.GetLogConsole(this).ClearConsole();
                    LogConsole.GetLogConsole(this).ShowConsole(true);
                    foreach (var a in fork.recordedActions) { Log.d(JsonWriter.AsPrettyString(a)); }
                });
        }
    }

    public static class Reducers {

        public static MyDataModel MainReducer(MyDataModel previousState, object action) {
            bool changed = false;
            var user = previousState.user.Mutate(action, ReduceUser, ref changed);
            if (changed) { return new MyDataModel() { user = user }; }
            return previousState;
        }

        private static MyUser ReduceUser(MyUser user, object a) {
            if (a is ChangeUName c1) { return user.DeepCopy(newUser => newUser.name = c1.name); }
            if (a is ChangeUIsHuman c2) { return user.DeepCopy(newUser => newUser.isHuman = c2.isHuman); }
            if (a is ChangeUAge c3) { return user.DeepCopy(newUser => newUser.age = c3.age); }
            return user;
        }

    }

    public class ChangeUName { public string name; }
    public class ChangeUIsHuman { public bool isHuman; }
    public class ChangeUAge { public int age; }

    [Serializable]
    public class MyDataModel {
        public MyUser user;
    }

    [Serializable]
    public class MyUser {
        public string name;
        public int age;
        internal bool isHuman;
    }

}