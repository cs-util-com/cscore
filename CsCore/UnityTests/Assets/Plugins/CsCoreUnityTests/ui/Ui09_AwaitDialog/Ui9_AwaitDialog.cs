using com.csutil.ui;
using NUnit.Framework;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace com.csutil.tests.ui {

    public class Ui9_AwaitDialog : MonoBehaviour { IEnumerator Start() { yield return new Ui9_AwaitDialogTests() { simulateUserInput = false }.ExampleUsage(); } }

    public class Ui9_AwaitDialogTests {

        public bool simulateUserInput = true;
        public int waitDurationPerDialogInMS = 500;

        [UnityTest]
        public IEnumerator ExampleUsage() {
            yield return ShowCancelConfirmDialog().AsCoroutine();
            yield return ShowInfoDialog().AsCoroutine();
            yield return ShowWarningDialog().AsCoroutine();
            yield return ShowErrorDialog().AsCoroutine();
            yield return UseDialogLoaderManually().AsCoroutine();
        }

        private async Task ShowCancelConfirmDialog() {
            var showDialogTask = ConfirmCancelDialog.Show("I am a dialog", "Please click the confirm button to continue");
            await SimulateConfirmButtonClick();
            bool dialogWasConfirmed = await showDialogTask; // Wait until the dialog is closed
            Assert.IsTrue(dialogWasConfirmed); // Check if user clicked confirm
        }

        private async Task ShowInfoDialog() {
            var showDialogTask = Dialog.ShowInfoDialog("I am an info dialog", "Please close me now to continue with the next dialog example");
            await SimulateConfirmButtonClick();
            await showDialogTask; // Wait until the dialog is closed
        }

        private async Task ShowWarningDialog() {
            var showDialogTask = Dialog.ShowWarningDialog("I am a warning", "Please close me now to continue with the next dialog example");
            await SimulateConfirmButtonClick();
            await showDialogTask; // Wait until the dialog is closed
        }

        private async Task ShowErrorDialog() {
            var showDialogTask = Dialog.ShowErrorDialog("I am an error", "Please close me now to continue with the next dialog example");
            await SimulateConfirmButtonClick();
            await showDialogTask; // Wait until the dialog is closed
        }

        /// <summary> This example shows how to use the DialogLoader manually to have full control over the UI presenter </summary>
        private async Task UseDialogLoaderManually() {
            var loader = new DialogLoader<ConfirmCancelDialog>(new ConfirmCancelDialog(caption: "I am a dialog",
                            message: "I can be awaited in the code, the async or coroutine can wait for the user " +
                            "to make a decision (select cancel or confirm) before the code continues!"));
            GameObject dialogUi = loader.LoadDialogPrefab(new ConfirmCancelDialog.DefaultPresenter(),
                            dialogPrefabName: "Dialogs/DefaultDialog1");
            RootCanvas.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Add dialog UI in a canvas
            var waitForUserInputInDialogTask = loader.ShowDialogAsync();
            Assert.IsFalse(loader.data.dialogWasConfirmed, "Dialog was already confirmed!");
            await SimulateConfirmButtonClick();
            ConfirmCancelDialog dialog = await waitForUserInputInDialogTask; // Wait until user clicks cancel or confirm
            Assert.IsTrue(dialog.dialogWasConfirmed, "Dialog was not confirmed!");
        }

        private async Task SimulateConfirmButtonClick() {
            if (simulateUserInput) {
                await TaskV2.Delay(waitDurationPerDialogInMS);
                Log.d("Now simulating the user clicking on the confirm button");
                RootCanvas.GetOrAddRootCanvas().gameObject.GetLinkMap().Get<Button>("ConfirmButton").onClick.Invoke();
            }
        }
    }

}
