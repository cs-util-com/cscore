using com.csutil.model.jsonschema;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class EnumFieldView : InputFieldView {

        public GameObject menuGo;
        public GameObject menuEntriesContainer;
        public GameObject menuEntryPrefab;

        protected override async Task Setup(string fieldName, string fullPath) {
            await base.Setup(fieldName, fullPath);
            input.interactable = field.additionalItems == true;
        }

        private void Start() {
            AssertV2.IsNotNull(field, "field", gameObject);
            CreateMenuEntryUis();
        }

        private void CreateMenuEntryUis() {
            foreach (string e in field.contentEnum) {
                var entry = menuEntriesContainer.AddChild(Instantiate(menuEntryPrefab));
                entry.GetComponentInChildren<Text>().textLocalized(e);
                entry.GetComponentInChildren<Button>().SetOnClickAction(delegate {
                    input.SetTextLocalizedWithNotify(e);
                    menuGo.SetActiveV2(false); // Hide menu
                });
            }

            if (field.additionalItems == true) { // If free text input possible, enable autocomplete
                // Collect all searchable UI text to filter entries when user types:
                var searchCache = menuEntriesContainer.GetChildrenIEnumerable()
                    .ToDictionary(x => x.GetComponentInChildren<Text>().text.ToLowerInvariant(), x => x);
                input.AddOnValueChangedAction(entered => {
                    entered = entered.ToLowerInvariant();
                    // Filter the suggestion array with each new char entered:
                    foreach (var child in searchCache) { child.Value.SetActiveV2(child.Key.Contains(entered)); }
                    return true;
                });
            }

        }

    }

}