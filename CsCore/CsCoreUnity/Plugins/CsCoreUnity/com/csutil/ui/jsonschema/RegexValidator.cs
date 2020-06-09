using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    /// <summary> A regex validator that evaluates a target input field and informs the user if 
    /// the entered data does not comply with its regex </summary>
    public class RegexValidator : MonoBehaviour {

        /// <summary> The regular expression that is used to evaluate the input </summary>
        public string regex;

        /// <summary> The target input that is monitored for changes and compared to the regex </summary>
        public InputField inputToValidate;

        /// <summary> If true then the input cant be null/emtpy </summary>
        public bool isInputRequired = false;

        /// <summary> The UI that is set to visible when an invalid input is detected </summary>
        public GameObject errorUi;

        /// <summary> The error text that the UI shows to the user when an invalid input is detected </summary>
        public Text errorText;

        /// <summary> The detail that is waited before the latest input is evaluated, 
        /// so that intermediate inputs are skipped while the user is still typing </summary>
        public double validationDelayInMs = 500;

        /// <summary> If >0 this will enforce the user to input at least this number of characters </summary>
        public int minLength;

        /// <summary> True if the current input matches the set regex </summary>
        private bool isInputValid = true;
        private Color? cachedOriginalColor;
        private UnityAction<string> inputListener;

        private void OnEnable() {
            inputListener = inputToValidate.AddOnValueChangedActionThrottled(EvalNewValue, validationDelayInMs);
        }

        private void OnDisable() {
            if (inputListener != null) { inputToValidate.onValueChanged.RemoveListener(inputListener); }
        }

        public void EnforceRegex(string regex) {
            if (enabled) { enabled = false; }
            this.regex = regex;
            enabled = true;
        }

        public bool CheckIfCurrentInputValid() {
            if (!enabled) { return true; }
            EvalNewValue(inputToValidate.text);
            return WasLatestCheckValid();
        }

        private bool WasLatestCheckValid() {
            if (isInputRequired && inputToValidate.text.IsNullOrEmpty()) { return false; }
            if (minLength > 0 && inputToValidate.text.Length < minLength) { return false; }
            return isInputValid;
        }

        /// <summary> Updates the state of the validator based on the new input </summary>
        private void EvalNewValue(string newValue) {
            isInputValid = regex.IsNullOrEmpty() || newValue.IsRegexMatch(regex);
            RefreshErrorUi();
        }

        private void RefreshErrorUi() {
            var wasLatestCheckValid = WasLatestCheckValid();
            errorUi.SetActiveV2(!wasLatestCheckValid);
            if (inputToValidate.targetGraphic != null) {
                if (!wasLatestCheckValid) {
                    if (errorText.color != inputToValidate.targetGraphic.color) {
                        cachedOriginalColor = inputToValidate.targetGraphic.color;
                        inputToValidate.targetGraphic.color = errorText.color;
                    }
                } else if (cachedOriginalColor != null) {
                    inputToValidate.targetGraphic.color = cachedOriginalColor.Value;
                }
            }
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