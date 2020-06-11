using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class BoolFieldView : FieldView {

        public Toggle toggle;

        protected override Task Setup(string fieldName, string fullPath) {
            toggle.interactable = field.readOnly != true;
            return Task.FromResult(true);
        }
    }

}