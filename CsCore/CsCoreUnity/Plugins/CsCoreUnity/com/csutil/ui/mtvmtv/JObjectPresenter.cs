using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class JObjectPresenter : Presenter<JObject> {
        public GameObject targetView { get; set; }
        public ViewModelToView vmtv;

        public JObjectPresenter(ViewModelToView vmtv) { this.vmtv = vmtv; }

        public async Task OnLoad(JObject root) {
            foreach (var fieldView in targetView.GetFieldViewMap().Values) {
                if (!fieldView.LinkToJsonModel(root)) {
                    if (fieldView is RecursiveFieldView r) {
                        r.ShowChildModelInNewScreen(root, targetView);
                    } else if (fieldView is ObjectFieldView) {
                        // Do nothing (object fields are individually set up themselves)
                    } else if (fieldView is ListFieldView l) {
                        await l.LoadModelList(root, vmtv);
                    } else {
                        Log.e($"Did not link {fieldView.GetType()}: {fieldView.fullPath}");
                    }
                }
            }
        }

    }

}