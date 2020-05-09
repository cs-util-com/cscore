using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class RegexValidator : MonoBehaviour {

        public string regex;
        public InputField inputToValidate;
        public bool isValidInput;
        public GameObject errorUi;
        public Text errorText;

        private Color? originalColor;
        private UnityAction<string> inputListener;

        public void EnforceRegex(string regex) {
            if (enabled) { enabled = false; }
            this.regex = regex;
            enabled = true;
        }

        private void OnEnable() {
            inputListener = inputToValidate.AddOnValueChangedAction((newValue) => {
                CheckIfValidInput(newValue);
                return true;
            });
        }

        public bool IsCurrentInputValid() {
            if (!enabled) { return true; }
            return CheckIfValidInput(inputToValidate.text);
        }

        private bool CheckIfValidInput(string newValue) {
            isValidInput = newValue.IsRegexMatch(regex);
            errorUi.SetActiveV2(!isValidInput);
            if (inputToValidate.targetGraphic != null) {
                if (!isValidInput) {
                    if (errorText.color != inputToValidate.targetGraphic.color) {
                        originalColor = inputToValidate.targetGraphic.color;
                        inputToValidate.targetGraphic.color = errorText.color;
                    }
                } else if (originalColor != null) {
                    inputToValidate.targetGraphic.color = originalColor.Value;
                }
            }
            return isValidInput;
        }

        private void OnDisable() {
            if (inputListener != null) { inputToValidate.onValueChanged.RemoveListener(inputListener); }
        }

    }

}