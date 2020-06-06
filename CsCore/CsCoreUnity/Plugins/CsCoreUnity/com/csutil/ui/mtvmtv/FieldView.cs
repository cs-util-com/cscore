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
        public Task OnViewCreated(string fieldName, string fullPath) {
            if (field != null) {
                title?.textLocalized(GetFieldTitle());
                if (!field.description.IsNullOrEmpty()) {
                    if (description == null) {
                        Log.w("No description UI set for the field view", gameObject);
                    } else {
                        description.textLocalized(field.description);
                    }
                }
            }
            if (!fullPath.IsNullOrEmpty()) { mainLink.SetId(fullPath); }
            return Setup(fieldName, fullPath);
        }

        public virtual string GetFieldTitle() {
            return field.title != null ? field.title : ViewModel.ToTitle(fieldName);
        }

        protected virtual Task Setup(string fieldName, string fullPath) {
            return Task.FromResult(false);
        }

    }

}