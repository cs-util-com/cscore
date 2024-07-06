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
            if (!ApplicationV2.isPlaying) {
                throw new System.NotSupportedException("Showing dialogs are only supported in play mode");
            }
            var dialog = new Dialog(caption, message, confirmText);
            var dialogLoader = new DialogLoader<Dialog>(dialog);
            var rootCanvas = RootCanvas.GetOrAddRootCanvasV2().gameObject;
            GameObject dialogUi = dialogLoader.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            rootCanvas.AddChild(dialogUi); // Add dialog UI in the root canvas
            var d = dialogLoader.ShowDialogAsync();
            EventBus.instance.Publish(EventConsts.catUi + UiEvents.DIALOG, dialog);
            return d;
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