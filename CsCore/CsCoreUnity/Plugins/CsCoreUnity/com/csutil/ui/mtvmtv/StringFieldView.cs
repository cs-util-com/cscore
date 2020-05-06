using System.Threading.Tasks;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class StringFieldView : FieldView {

        public InputField input;

        protected override Task Setup(string fieldName) {
            input.interactable = field.readOnly != true;
            return Task.FromResult(true);
        }
    }

}