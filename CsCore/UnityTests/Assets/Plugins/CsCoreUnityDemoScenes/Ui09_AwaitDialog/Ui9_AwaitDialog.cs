using com.csutil.ui;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.tests.ui {

    public class Ui9_AwaitDialog : UnitTestMono {

        public int waitDurationPerDialogInMS = 500;

        public override IEnumerator RunTest() { yield return ShowAllDialogs().AsCoroutine(); }

        private async Task ShowAllDialogs() {
            AssertVisually.SetupDefaultSingletonInDebugMode();
            await ShowCancelConfirmDialog();
            await ShowInfoDialog();
            await ShowWarningDialog();
            await ShowErrorDialog();
            await UseDialogLoaderManually();
        }

        private async Task ShowCancelConfirmDialog() {
            var showDialogTask = ConfirmCancelDialog.Show("I am a dialog", "Please click the confirm button to continue");
            await SimulateConfirmButtonClick();
            bool dialogWasConfirmed = await showDialogTask; // Wait until the dialog is closed
            AssertV2.IsTrue(dialogWasConfirmed, "User did not confirm dialog"); // Check if user clicked confirm
        }

        private async Task ShowInfoDialog() {
            var showDialogTask = Dialog.ShowInfoDialog("I am an info dialog", "Please close me now to continue with the next dialog example", "Close");
            await SimulateConfirmButtonClick();
            await showDialogTask; // Wait until the dialog is closed
        }

        private async Task ShowWarningDialog() {
            var showDialogTask = Dialog.ShowWarningDialog("I am a warning", "Please close me now to continue with the next dialog example", "Oook");
            await SimulateConfirmButtonClick();
            await showDialogTask; // Wait until the dialog is closed
        }

        private async Task ShowErrorDialog() {
            var showDialogTask = Dialog.ShowErrorDialog("I am an error", "Please close me now to continue with the next dialog example", "Oh noo!");
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
            AssertV2.IsFalse(loader.data.dialogWasConfirmed, "Dialog was already confirmed!");
            await SimulateConfirmButtonClick();
            ConfirmCancelDialog dialog = await waitForUserInputInDialogTask; // Wait until user clicks cancel or confirm
            AssertV2.IsTrue(dialog.dialogWasConfirmed, "Dialog was not confirmed!");
        }

        private async Task SimulateConfirmButtonClick() {
            await TaskV2.Delay(waitDurationPerDialogInMS);
            SimulateButtonClickOn("ConfirmButton");
        }

    }

}
