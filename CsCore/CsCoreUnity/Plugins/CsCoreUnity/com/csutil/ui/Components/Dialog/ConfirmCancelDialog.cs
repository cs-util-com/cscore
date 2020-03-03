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

        public static Task<ConfirmCancelDialog> Show(string caption, string message, string confirmBtnText = null, string dialogPrefabName = "Dialogs/DefaultDialog1") {
            var dialog = new DialogLoader<ConfirmCancelDialog>(new ConfirmCancelDialog(caption, message, confirmBtnText));
            GameObject dialogUi = dialog.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            CanvasFinder.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Add dialog UI in a canvas
            return dialog.ShowDialogAsync();
        }

        public class DefaultPresenter : Presenter<ConfirmCancelDialog> {

            public GameObject targetView { get; set; }

            public async Task OnLoad(ConfirmCancelDialog dialogData) {
                // Setup the dialog UI (& fill it with the data):
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").text = dialogData.caption;
                links.Get<Text>("Message").text = dialogData.message;
                if (!dialogData.confirmBtnText.IsNullOrEmpty()) { links.Get<Text>("ConfirmButton").text = dialogData.confirmBtnText; }
                var cancelTask = links.Get<Button>("CancelButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = false; });
                var confirmTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = true; });

                await Task.WhenAny(cancelTask, confirmTask); // Wait for the user to make a choise (cancel or confirm)
                targetView.Destroy(); // Now that the user made his choise the dialog can be closed
            }

        }

    }

}
