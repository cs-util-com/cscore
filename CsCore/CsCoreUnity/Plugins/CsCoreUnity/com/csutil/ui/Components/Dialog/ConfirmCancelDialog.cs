using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class ConfirmCancelDialog {

        public string caption;
        public string message;
        public string confirmBtnText;
        public bool dialogWasConfirmed = false;

        public ConfirmCancelDialog(string caption, string message, string confirmBtnText = null) {
            this.caption = caption;
            this.message = message;
            this.confirmBtnText = confirmBtnText;
        }

        public static async Task<bool> Show(string caption, string message, string confirmBtnText = null, string dialogPrefabName = "Dialogs/DefaultDialog2") {
            var loader = new DialogLoader<ConfirmCancelDialog>(new ConfirmCancelDialog(caption, message, confirmBtnText));
            GameObject dialogUi = loader.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            RootCanvas.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Add dialog UI in a canvas
            ConfirmCancelDialog dialog = await loader.ShowDialogAsync();
            return dialog.dialogWasConfirmed;
        }

        public class DefaultPresenter : Presenter<ConfirmCancelDialog> {

            public GameObject targetView { get; set; }

            public Task OnLoad(ConfirmCancelDialog dialogData) {
                // Setup the dialog UI (& fill it with the data):
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").text = dialogData.caption;
                links.Get<Text>("Message").text = dialogData.message;
                if (!dialogData.confirmBtnText.IsNullOrEmpty()) { links.Get<Text>("ConfirmButton").text = dialogData.confirmBtnText; }
                var cancelTask = links.Get<Button>("CancelButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = false; });
                var confirmTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = true; });

                return Task.WhenAny(cancelTask, confirmTask); // Wait for the user to make a choise (cancel or confirm)
            }

        }

    }

}
