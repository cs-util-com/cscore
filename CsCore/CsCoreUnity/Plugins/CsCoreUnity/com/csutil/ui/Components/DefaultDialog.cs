using com.csutil.ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil {

    public class Dialog<T> {

        private Presenter<T> presenter;
        public T data;
        public Dialog(T dialogData) { this.data = dialogData; }

        public GameObject LoadDialogPrefab(Presenter<T> dialogPresenter, string dialogPrefabName) {
            presenter = dialogPresenter;
            presenter.targetView = ResourcesV2.LoadPrefab(dialogPrefabName);
            return presenter.targetView;
        }

        // Show the data in the dialog and get back the task that can be awaited (will finish when the user made a decision):
        public Task<T> ShowDialogAsync() {
            if (presenter == null) { throw Log.e("dialog.CreateDialogPrefab() has to be called first"); }
            return presenter.LoadModelIntoView(data);
        }

    }

    public class ConfirmCancelDialog {
        public string caption;
        public string message;
        public bool dialogWasConfirmed = false;

        public ConfirmCancelDialog(string caption, string message) {
            this.caption = caption;
            this.message = message;
        }
    }

    public class ConfirmCancelDialogPresenter : Presenter<ConfirmCancelDialog> {

        public GameObject targetView { get; set; }

        public async Task OnLoad(ConfirmCancelDialog dialogData) {
            // Setup the dialog UI (& fill it with the data):
            var links = targetView.GetLinkMap();
            links.Get<Text>("Caption").text = dialogData.caption;
            links.Get<Text>("Message").text = dialogData.message;
            var cancelTask = links.Get<Button>("CancelButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = false; });
            var confirmTask = links.Get<Button>("ConfirmButton").SetOnClickAction(delegate { dialogData.dialogWasConfirmed = true; });

            await Task.WhenAny(cancelTask, confirmTask); // Wait for the user to make a choise (cancel or confirm)
            targetView.Destroy(); // Now that the user made his choise the dialog can be closed
            Log.d("The dialog was confirmed=" + dialogData.dialogWasConfirmed);
        }

    }

}
