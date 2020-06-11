using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class Dialog {

        public string caption;
        public string message;
        public string confirmText;

        public Dialog(string caption, string message, string confirmText) {
            this.caption = caption;
            this.message = message;
            this.confirmText = confirmText;
        }

        public static Task ShowInfoDialog(string caption, string message, string confirmText, string dialogPrefabName = "Dialogs/InfoDialog1") {
            return Show(caption, message, confirmText, dialogPrefabName);
        }

        public static Task ShowWarningDialog(string caption, string message, string confirmText, string dialogPrefabName = "Dialogs/WarningDialog1") {
            return Show(caption, message, confirmText, dialogPrefabName);
        }

        public static Task ShowErrorDialog(string caption, string message, string confirmText, string dialogPrefabName = "Dialogs/ErrorDialog1") {
            return Show(caption, message, confirmText, dialogPrefabName);
        }

        private static Task Show(string caption, string message, string confirmText, string dialogPrefabName) {
            var dialog = new DialogLoader<Dialog>(new Dialog(caption, message, confirmText));
            GameObject dialogUi = dialog.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            RootCanvas.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Add dialog UI in a canvas
            return dialog.ShowDialogAsync();
        }

        public class DefaultPresenter : Presenter<Dialog> {

            public GameObject targetView { get; set; }

            public Task OnLoad(Dialog dialogData) {
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").textLocalized(dialogData.caption);
                links.Get<Text>("Message").textLocalized(dialogData.message);
                Button confirmButton = links.Get<Button>("ConfirmButton");
                if (!dialogData.confirmText.IsNullOrEmpty()) {
                    confirmButton.GetComponentInChildren<Text>().textLocalized(dialogData.confirmText);
                }
                return confirmButton.SetOnClickAction(delegate { }); // Wait for the user to click
            }

        }

    }

}