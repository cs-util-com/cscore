using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.mtvmtv {

    public class ObjectFieldView : FieldView {

        public CanvasGroup canvasGroup;

        protected override Task Setup(string fieldName) {
            canvasGroup.interactable = field.readOnly != true;
            return Task.FromResult(true);
        }

    }

}