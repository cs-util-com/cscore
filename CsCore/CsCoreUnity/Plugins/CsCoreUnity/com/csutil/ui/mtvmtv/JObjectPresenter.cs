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
                var value = fieldView.GetFieldJModel(root);
                if (!fieldView.LinkToJsonModel(root, value)) {
                    if (fieldView is RecursiveFieldView r) {
                        r.ShowChildModelInNewScreen(targetView, value as JObject);
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