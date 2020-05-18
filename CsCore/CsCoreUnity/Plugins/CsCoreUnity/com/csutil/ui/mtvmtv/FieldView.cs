using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class FieldView : MonoBehaviour {

        public Text title;
        public Text description;
        public Link mainLink;

        [SerializeField]
        public ViewModel field;
        public string fieldName;
        public string fullPath;

        /// <summary> Will be called by the ViewModelToView logic when the view created </summary>
        public virtual Task OnViewCreated(string fieldName, string fullPath) {
            if (field != null) {
                title.textLocalized(field.title);
                if (!field.description.IsNullOrEmpty()) {
                    if (description == null) {
                        Log.w("No description UI set for the field view", gameObject);
                    } else {
                        description.textLocalized(field.description);
                    }
                }
            }
            mainLink.id = fullPath;
            return Setup(fieldName, fullPath);
        }

        protected virtual Task Setup(string fieldName, string fullPath) {
            return Task.FromResult(false);
        }

    }

}