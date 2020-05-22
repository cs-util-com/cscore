using System;
using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class ListFieldView : FieldView {

        protected override Task Setup(string fieldName, string fullPath) {
            throw new NotImplementedException();
            return Task.FromResult(true);
        }

        internal void OnObjectArray(ViewModel entryViewModel) {
            throw new NotImplementedException();
        }
    }

}