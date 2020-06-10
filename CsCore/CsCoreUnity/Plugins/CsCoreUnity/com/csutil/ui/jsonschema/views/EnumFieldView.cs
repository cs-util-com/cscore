using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class EnumFieldView : InputFieldView {

        public GameObject menuGo;
        public GameObject menuEntriesContainer;
        public GameObject menuEntryPrefab;
        public ToggleGoVisibility menuOnClickVisibilityToggle;

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
                    .ToDictionary(x => x.GetComponentInChildren<Text>().text, x => x);
                input.AddOnValueChangedAction(entered => { // Filter suggestions for each entered char:
                    entered = entered.ToLowerInvariant();
                    foreach (var child in searchCache) {
                        child.Value.SetActiveV2(child.Key.ToLowerInvariant().Contains(entered));
                    }
                    return true;
                });
                input.onEndEdit.AddListener(_ => {
                    var x = searchCache.Filter(xx => xx.Value != null && xx.Value.activeSelf);
                    if (x.Count() == 1) {
                        input.text = x.First().Key;
                        ToggleMenuVisibility();
                    }
                });
            }

        }

        private void ToggleMenuVisibility() { menuOnClickVisibilityToggle.ToggleVisibilityOfTarget(); }

        public void ShowAllDropDownMenuEntries() {
            foreach (var c in menuEntriesContainer.GetChildren()) { c.SetActiveV2(true); }
        }

        public void OnMenuToggled(bool isOpen) { if (isOpen) { ShowAllDropDownMenuEntries(); } }

    }

}