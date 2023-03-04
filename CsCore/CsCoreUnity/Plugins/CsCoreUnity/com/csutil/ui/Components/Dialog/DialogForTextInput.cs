using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class DialogForTextInput {

        public const string TEXT_INPUT_FIELD_ID = "UserInput";

        public string caption;
        public string message;
        public string confirmBtnText;
        public DialogResult dialogResult;
        public Func<string, bool> validate;

        public DialogForTextInput(string caption, string message, string confirmBtnText = null, Func<string, bool> validate = null, string enteredText = null) {
            this.caption = caption;
            this.message = message;
            this.confirmBtnText = confirmBtnText;
            dialogResult.enteredText = enteredText;
            this.validate = validate;
        }

        public struct DialogResult {
            public string enteredText;
            public bool dialogWasConfirmed;
        }

        public static async Task<DialogResult> Show(string caption, string message, string confirmBtnText = null, Func<string, bool> validate = null, string dialogPrefabName = "Dialogs/DefaultInputDialog1") {
            var loader = new DialogLoader<DialogForTextInput>(new DialogForTextInput(caption, message, confirmBtnText, validate));
            var rootCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject;
            GameObject dialogUi = loader.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            rootCanvas.AddChild(dialogUi); // Add dialog UI in a canvas
            DialogForTextInput dialog = await loader.ShowDialogAsync();
            EventBus.instance.Publish(EventConsts.catUi + UiEvents.INPUT_DIALOG, dialog);
            return dialog.dialogResult;
        }

        public class DefaultPresenter : Presenter<DialogForTextInput> {

            public GameObject targetView { get; set; }
            
            private bool userWantsToCancelDialog = false;

            public Task OnLoad(DialogForTextInput dialogData) {
                // Setup the dialog UI (& fill it with the data):
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").text = dialogData.caption;
                links.Get<Text>("Message").text = dialogData.message;
                if (!dialogData.confirmBtnText.IsNullOrEmpty()) { links.Get<Text>("ConfirmButtonText").text = dialogData.confirmBtnText; }

                var textUi = links.Get<InputField>(TEXT_INPUT_FIELD_ID);
                if (!dialogData.dialogResult.enteredText.IsNullOrEmpty()) { textUi.text = dialogData.dialogResult.enteredText; }
                textUi.SetOnValueChangedActionThrottled(newInput => {
                    if (IsUserInputValid(dialogData, newInput)) { dialogData.dialogResult.enteredText = newInput; }
                });
                return WaitForCancelOrOk(dialogData, links, textUi);
            }

            private static bool IsUserInputValid(DialogForTextInput dialogData, string newInput) { return dialogData.validate == null || dialogData.validate(newInput); }

            private async Task WaitForCancelOrOk(DialogForTextInput dialogData, Dictionary<string, Link> links, InputField inputField) {
                var cancelTask = links.Get<Button>("CancelButton").SetOnClickAction(delegate {
                    dialogData.dialogResult.dialogWasConfirmed = false;
                    userWantsToCancelDialog = true;
                });
                var confirmTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate {
                    if (IsUserInputValid(dialogData, inputField.text)) {
                        dialogData.dialogResult.enteredText = inputField.text;
                        dialogData.dialogResult.dialogWasConfirmed = true;
                    }
                });
                await Task.WhenAny(cancelTask, confirmTask); // Wait for the user to make a choise (cancel or confirm)

                // If any of the 2 buttons were clicked, but the input is not approved, continue waiting: 
                if (!userWantsToCancelDialog && !IsUserInputValid(dialogData, inputField.text)) {
                    await WaitForCancelOrOk(dialogData, links, inputField);
                }
            }

        }

    }

}