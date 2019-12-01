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

            var targetCanvas = CanvasFinder.GetOrAddRootCanvas().gameObject;
            var dialogUi = targetCanvas.AddChild(ResourcesV2.LoadPrefab("Dialogs/DefaultDialog1"));

            MyDialog1Presenter userUiPresenter = new MyDialog1Presenter();
            userUiPresenter.targetView = dialogUi;

            var dialogData = new MyDialog1Data() { caption = "I am a dialog", message = "Some shorter text as a dialog message..." };
            yield return userUiPresenter.LoadModelIntoView(dialogData).AsCoroutine();

            Log.d("dialogData.dialogWasConfirmed=" + dialogData.dialogWasConfirmed);

        }

        [System.Serializable]
        public class MyDialog1Data {
            public string caption;
            public string message;
            internal bool dialogWasConfirmed = false;

            public override string ToString() { return JsonWriter.GetWriter().Write(this); }
        }

        public class MyDialog1Presenter : Presenter<MyDialog1Data> {

            public GameObject targetView { get; set; }

            public async Task OnLoad(MyDialog1Data dialogData) {
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").text = dialogData.caption;
                links.Get<Text>("Message").text = dialogData.message;
                var cancelTask = links.Get<Button>("CancelButton").SetOnClickAction(delegate {
                    dialogData.dialogWasConfirmed = false;
                    targetView.Destroy();
                });
                var confirmTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate {
                    dialogData.dialogWasConfirmed = true;
                    targetView.Destroy();
                });
                await Task.WhenAny(cancelTask, confirmTask);
                Log.d("dialogWasConfirmed=" + dialogData.dialogWasConfirmed);
            }

        }

    }

}
