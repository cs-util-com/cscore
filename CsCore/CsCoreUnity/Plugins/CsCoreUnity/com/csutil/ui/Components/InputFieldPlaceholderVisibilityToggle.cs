using UnityEngine;

namespace com.csutil.ui.Components {

    public class InputFieldPlaceholderVisibilityToggle : MonoBehaviour {

        public MonoBehaviour otherPlaceholder;

        public void OnEnable() {
            otherPlaceholder.ThrowErrorIfNull("otherPlaceholder");
            // When the other placeholder is enabled, this placeholder should be disabled and vice versa:
            gameObject.SetActiveV2(!otherPlaceholder.enabled);
            gameObject.GetParent().GetComponent<TMPro.TMP_InputField>().onValueChanged.AddListener(_ => {
                gameObject.SetActiveV2(!otherPlaceholder.enabled);
            });
        }

    }

}