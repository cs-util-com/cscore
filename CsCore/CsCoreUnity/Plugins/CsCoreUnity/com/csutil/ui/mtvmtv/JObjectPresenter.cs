using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class JObjectPresenter : Presenter<JObject> {
        public GameObject targetView { get; set; }

        public Task OnLoad(JObject root) {
            foreach (var fieldView in targetView.GetFieldViewMap().Values) {
                if (!fieldView.LinkToJsonModel(root)) {
                    if (fieldView is RecursiveModelField r) {
                        r.ShowChildModelInNewScreen(root, targetView);
                    } else if (fieldView is ObjectFieldView) {
                        // Do nothing (object fields are individually set up themselves)
                    } else {
                        Log.e($"Did not link {fieldView.GetType()}: {fieldView.fullPath}");
                    }
                }
            }
            return Task.FromResult(true);
        }

    }

}