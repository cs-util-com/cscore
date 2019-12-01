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
using com.csutil;

namespace com.csutil.tests.ui {

    public class Ui9_AwaitDialog : MonoBehaviour { IEnumerator Start() { yield return new Ui9_AwaitDialogTests() { simulateUserInput = false }.ExampleUsage(); } }

    public class Ui9_AwaitDialogTests {

        public bool simulateUserInput = true;

        [UnityTest]
        public IEnumerator ExampleUsage() {

            DefaultDialog dialog = new DefaultDialog() { caption = "I am a dialog", message = "Some shorter text as a dialog message..." };
            GameObject dialogUi = dialog.CreateDialogPrefab();
            CanvasFinder.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Show dialog UI in a canvas
            Task waitForDialogTask = dialog.ShowDialogAsync();

            if (simulateUserInput) {
                dialogUi.GetComponent<MonoBehaviour>().ExecuteDelayed(() => {
                    Log.d("Now simulating the user clicking on the confirm button");
                    dialogUi.GetLinkMap().Get<Button>("ConfirmButton").onClick.Invoke();
                }, delayInMsBeforeExecution: 1);
            }

            Assert.IsFalse(dialog.dialogWasConfirmed, "Dialog was already confirmed!");
            yield return waitForDialogTask.AsCoroutine();
            Assert.IsTrue(dialog.dialogWasConfirmed, "Dialog was not confirmed!");

        }

    }

}
