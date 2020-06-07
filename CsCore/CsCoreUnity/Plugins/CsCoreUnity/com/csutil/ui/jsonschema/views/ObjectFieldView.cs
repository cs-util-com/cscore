using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil.ui.jsonschema {

    public class ObjectFieldView : FieldView {

        public CanvasGroup canvasGroup;

        protected override Task Setup(string fieldName, string fullPath) {
            canvasGroup.interactable = field.readOnly != true;
            return Task.FromResult(true);
        }

    }

}