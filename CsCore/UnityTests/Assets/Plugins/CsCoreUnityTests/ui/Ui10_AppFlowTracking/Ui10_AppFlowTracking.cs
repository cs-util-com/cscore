using com.csutil;
using com.csutil.model.immutable;
using com.csutil.ui;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui10_AppFlowTracking : MonoBehaviour {

        IEnumerator Start() {

            AppFlow.instance = new TestAppFlowTracker();
            AppFlow.instance.ActivateLinkMapTracking();
            AppFlow.instance.ActivatePrefabLoadTracking();
            AppFlow.instance.ActivateUiEventTracking();
            AppFlow.instance.ActivateViewStackTracking();

            var p = new Ui10_AppFlowTrackingTests.MyDataModelPresenter();
            p.targetView = gameObject; // Assume that Ui10_AppFlowTracking is attached to the correct UI
            yield return new Ui10_AppFlowTrackingTests() { presenter = p }.ExampleUsage();
        }

    }

    class TestAppFlowTracker : IAppFlow {
        public void TrackEvent(string category, string action, params object[] args) {
            Log.d("AppFlowTests: " + category + " - " + action);
        }
    }

    public class Ui10_AppFlowTrackingTests {

        public MyDataModelPresenter presenter;

        [UnityTest]
        public IEnumerator ExampleUsage() {
            setupImmutableDatastore();

            GetStore().Dispatch(new ActionSetBool1() { newB = true });
            GetStore().Dispatch(new ActionSetString1 { newS = "abc" });

            Assert.AreEqual("abc", GetStore().GetState().string1);
            Assert.AreEqual(true, GetStore().GetState().bool1);

            if (presenter != null) {
                yield return presenter.LoadModelIntoView(GetStore().GetState()).AsCoroutine();
            }

            yield return null;
        }

        private IDataStore<MyDataModel> GetStore() { return IoC.inject.Get<IDataStore<MyDataModel>>(this); }

        private void setupImmutableDatastore() {
            Log.MethodEntered();
            var log = Middlewares.NewAppFlowTrackerMiddleware<MyDataModel>();
            IDataStore<MyDataModel> store = new DataStore<MyDataModel>(MainReducer, new MyDataModel(string1: "", bool1: false), log);
            IoC.inject.SetSingleton(store);
        }

        public class MyDataModel {
            public readonly bool bool1;
            public readonly string string1;
            public MyDataModel(string string1, bool bool1) {
                this.string1 = string1;
                this.bool1 = bool1;
            }
        }

        public class MyDataModelPresenter : Presenter<MyDataModel> {
            public GameObject targetView { get; set; }

            public Task OnLoad(MyDataModel model) {
                var map = targetView.GetLinkMap();
                map.Get<InputField>("string1").text = model.string1;
                map.Get<InputField>("string1").SetOnValueChangedAction((newVal) => {
                    if (newVal.IsNullOrEmpty()) { return false; }
                    return true;
                });
                map.Get<Toggle>("bool1").isOn = model.bool1;
                map.Get<Toggle>("bool1").SetOnValueChangedAction((newVal) => {
                    return true;
                });
                map.Get<Button>("ShowDialog").SetOnClickAction(async (b) => {
                    var dialog = await ConfirmCancelDialog.Show("I am a dialog", "Current model.string1=" + model.string1);
                    Log.d("Dialog was confirmed: " + dialog.dialogWasConfirmed);
                });
                return Task.CompletedTask;
            }

        }

        private class ActionSetString1 { public string newS; }
        private class ActionSetBool1 { public bool newB; }

        private MyDataModel MainReducer(MyDataModel prev, object action) {
            if (action is ActionSetBool1 a1) { return new MyDataModel(prev.string1, a1.newB); }
            if (action is ActionSetString1 a2) { return new MyDataModel(a2.newS, prev.bool1); }
            return prev;
        }

    }

}