using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class Dialog {

        public string caption;
        public string message;

        public Dialog(string caption, string message) {
            this.caption = caption;
            this.message = message;
        }

        public static Task<Dialog> ShowInfoDialog(string caption, string message, string dialogPrefabName = "Dialogs/InfoDialog1") {
            return Show(caption, message, dialogPrefabName);
        }

        public static Task<Dialog> ShowWarningDialog(string caption, string message, string dialogPrefabName = "Dialogs/WarningDialog1") {
            return Show(caption, message, dialogPrefabName);
        }

        public static Task<Dialog> ShowErrorDialog(string caption, string message, string dialogPrefabName = "Dialogs/ErrorDialog1") {
            return Show(caption, message, dialogPrefabName);
        }

        private static Task<Dialog> Show(string caption, string message, string dialogPrefabName) {
            var dialog = new DialogLoader<Dialog>(new Dialog(caption, message));
            GameObject dialogUi = dialog.LoadDialogPrefab(new DefaultPresenter(), dialogPrefabName);
            CanvasFinder.GetOrAddRootCanvas().gameObject.AddChild(dialogUi); // Add dialog UI in a canvas
            return dialog.ShowDialogAsync();
        }

        public class DefaultPresenter : Presenter<Dialog> {

            public GameObject targetView { get; set; }

            public async Task OnLoad(Dialog dialogData) {
                var links = targetView.GetLinkMap();
                links.Get<Text>("Caption").text = dialogData.caption;
                links.Get<Text>("Message").text = dialogData.message;
                var cancelTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate { });
                targetView.Destroy(); // Close dialog after user confirmed it
            }

        }

    }

}