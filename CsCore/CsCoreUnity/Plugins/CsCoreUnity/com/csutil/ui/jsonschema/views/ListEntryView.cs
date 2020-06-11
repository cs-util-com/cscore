using com.csutil.model;
using com.csutil.model.jsonschema;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class ListEntryView : InputFieldView {

        public Toggle checkmark;

        protected override async Task Setup(string fieldName, string fullPath) {
            await base.Setup(fieldName, fullPath);
        }

    }

}
