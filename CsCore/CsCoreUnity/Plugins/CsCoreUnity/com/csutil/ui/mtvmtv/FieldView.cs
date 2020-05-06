using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class FieldView : MonoBehaviour {

        public Text title;
        public Text description;
        public Link mainLink;

        public string fieldName;
        [SerializeField]
        public ViewModel.Field field;

        /// <summary> Will be called by the ViewModelToView logic when the view created </summary>
        public virtual Task OnViewCreated(string fieldName) {
            this.fieldName = fieldName;
            if (field != null) {
                title.textLocalized(field.text.name);
                if (!field.text.descr.IsNullOrEmpty()) {
                    if (description == null) {
                        Log.e("No description UI set for the field view", this);
                    } else {
                        description.textLocalized(field.text.descr);
                    }
                }
            }
            mainLink.id = fieldName;
            return Setup(fieldName);
        }

        protected virtual Task Setup(string fieldName) {
            return Task.FromResult(false);
        }

    }

}