using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class RegexValidator : MonoBehaviour {

        public string regex;
        public InputField inputToValidate;
        /// <summary> If true then the input cant be null/emtpy </summary>
        public bool isInputRequired = false;
        public GameObject errorUi;
        public Text errorText;
        public double validationDelayInMs = 500;

        /// <summary> True if the current input matches the set regex </summary>
        private bool isInputValid = true;
        private Color? originalColor;
        private UnityAction<string> inputListener;

        public void EnforceRegex(string regex) {
            if (enabled) { enabled = false; }
            this.regex = regex;
            enabled = true;
        }

        private void OnEnable() {
            inputListener = inputToValidate.AddOnValueChangedActionThrottled(EvalNewValue, validationDelayInMs);
        }

        public bool CheckIfCurrentInputValid() {
            if (!enabled) { return true; }
            EvalNewValue(inputToValidate.text);
            return WasLatestCheckValid();
        }

        private bool WasLatestCheckValid() {
            if (isInputRequired && inputToValidate.text.IsNullOrEmpty()) { return false; }
            return isInputValid;
        }

        /// <summary> Updates the state of the validator based on the new input </summary>
        private void EvalNewValue(string newValue) {
            isInputValid = regex == null || newValue.IsRegexMatch(regex);
            RefreshErrorUi();
        }

        private void RefreshErrorUi() {
            var wasLatestCheckValid = WasLatestCheckValid();
            errorUi.SetActiveV2(!wasLatestCheckValid);
            if (inputToValidate.targetGraphic != null) {
                if (!wasLatestCheckValid) {
                    if (errorText.color != inputToValidate.targetGraphic.color) {
                        originalColor = inputToValidate.targetGraphic.color;
                        inputToValidate.targetGraphic.color = errorText.color;
                    }
                } else if (originalColor != null) {
                    inputToValidate.targetGraphic.color = originalColor.Value;
                }
            }
        }

        private void OnDisable() {
            if (inputListener != null) { inputToValidate.onValueChanged.RemoveListener(inputListener); }
        }

        public static bool IsAllInputCurrentlyValid(GameObject view) {
            var allFoundValidators = view.GetComponentsInChildren<RegexValidator>();
            var invalidFields = allFoundValidators.Filter(v => !v.WasLatestCheckValid());
            if (invalidFields.IsNullOrEmpty()) { return true; }
            var f = invalidFields.First();
            f.RefreshErrorUi();
            f.inputToValidate.SelectV2(); // Set focus on invalid field
            var errorText = f.errorText?.text;
            if (!errorText.IsNullOrEmpty()) { Toast.Show(errorText); }
            return false;
        }

    }

}