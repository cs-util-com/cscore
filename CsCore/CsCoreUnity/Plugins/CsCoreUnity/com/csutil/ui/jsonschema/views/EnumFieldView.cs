using com.csutil.model.jsonschema;
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
                    input.SetTextWithNotify(e);
                    menuGo.SetActiveV2(false); // Hide menu
                });
            }
        }

    }

}