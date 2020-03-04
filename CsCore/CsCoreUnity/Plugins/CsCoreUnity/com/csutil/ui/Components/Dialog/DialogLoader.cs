using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui {

    public class DialogLoader<T> {

        private Presenter<T> presenter;
        public T data;
        public DialogLoader(T dialogData) { this.data = dialogData; }

        public GameObject LoadDialogPrefab(Presenter<T> dialogPresenter, string dialogPrefabName) {
            presenter = dialogPresenter;
            presenter.targetView = ResourcesV2.LoadPrefab(dialogPrefabName);
            return presenter.targetView;
        }

        // Show the data in the dialog and get back the task that can be awaited (will finish when the user made a decision):
        public async Task<T> ShowDialogAsync() {
            if (presenter == null) { throw Log.e("dialog.CreateDialogPrefab() has to be called first"); }
            T data = await presenter.LoadModelIntoView(this.data);
            presenter.targetView.Destroy(); // Close dialog after user is done with it
            return data;
        }

    }

}
