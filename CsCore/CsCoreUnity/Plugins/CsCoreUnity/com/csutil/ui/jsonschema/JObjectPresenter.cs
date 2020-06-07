using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class JObjectPresenter : Presenter<JObject> {
        public GameObject targetView { get; set; }
        public ViewModelToView vmtv;

        public JObjectPresenter(ViewModelToView vmtv) { this.vmtv = vmtv; }

        public async Task OnLoad(JObject root) { await targetView.LinkToJsonModel(root, vmtv); }

    }

}