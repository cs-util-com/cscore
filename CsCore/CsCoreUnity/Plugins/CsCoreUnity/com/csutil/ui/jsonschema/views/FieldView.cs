using System.Threading.Tasks;
using com.csutil.model.jsonschema;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class FieldView : MonoBehaviour, ISerializationCallbackReceiver {

        public Text title;
        public Text description;
        public Link mainLink;

        public string fieldName;
        public string fullPath;

        public string fieldAsJson;
        public JsonSchema field;

        public void OnBeforeSerialize() {
            if (field == null) { fieldAsJson = null; return; }
            fieldAsJson = JsonWriter.GetWriter().Write(field);
        }

        public void OnAfterDeserialize() {
            if (!fieldAsJson.IsNullOrEmpty()) { field = JsonReader.GetReader().Read<JsonSchema>(fieldAsJson); }
        }

        /// <summary> Will be called by the JsonSchemaToView logic when the view created </summary>
        public Task OnViewCreated(string fieldName, string fullPath) {
            if (field != null) {
                title?.textLocalized(GetFieldTitle());
                if (!field.description.IsNullOrEmpty()) {
                    if (description == null) {
                        Log.w($"No description UI set for field view '{fullPath}'", gameObject);
                    } else {
                        description.textLocalized(field.description);
                    }
                }
            }
            if (!fullPath.IsNullOrEmpty()) { mainLink.SetId(fullPath); }
            return Setup(fieldName, fullPath);
        }

        public virtual string GetFieldTitle() {
            return field.title != null ? field.title : JsonSchema.ToTitle(fieldName);
        }

        protected virtual Task Setup(string fieldName, string fullPath) {
            return Task.FromResult(false);
        }

    }

}