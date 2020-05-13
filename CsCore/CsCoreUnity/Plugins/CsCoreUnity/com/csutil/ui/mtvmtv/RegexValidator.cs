using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class RegexValidator : MonoBehaviour {

        public string[] regex;
        public InputField inputToValidate;
        public bool isValidInput = true;
        public GameObject errorUi;
        public Text errorText;
        public double validationDelayInMs = 500;

        private Color? originalColor;
        private UnityAction<string> inputListener;

        public void EnforceRegex(string[] regex) {
            if (enabled) { enabled = false; }
            this.regex = regex;
            enabled = true;
        }

        private void OnEnable() {
            inputListener = inputToValidate.AddOnValueChangedActionThrottled((newValue) => {
                CheckIfValidInput(newValue);
            }, validationDelayInMs);
        }

        public bool IsCurrentInputValid() {
            if (!enabled) { return true; }
            return CheckIfValidInput(inputToValidate.text);
        }

        private bool CheckIfValidInput(string newValue) {
            foreach (var r in regex) {
                isValidInput = newValue.IsRegexMatch(r);
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
                if (!isValidInput) { return false; }
            }
            return true;
        }

        private void OnDisable() {
            if (inputListener != null) { inputToValidate.onValueChanged.RemoveListener(inputListener); }
        }

        public static bool IsAllInputCurrentlyValid(GameObject view) {
            var allFoundValidators = view.GetComponentsInChildren<RegexValidator>();
            var invalidFields = allFoundValidators.Filter(v => !v.isValidInput);
            if (invalidFields.IsNullOrEmpty()) { return true; }
            var f = invalidFields.First();
            f.inputToValidate.SelectV2(); // Set focus on invalid field
            var errorText = f.errorText?.text;
            if (!errorText.IsNullOrEmpty()) { Toast.Show(errorText); }
            return false;
        }

    }

}