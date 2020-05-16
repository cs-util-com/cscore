using com.csutil.model.mtvmtv;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class InputFieldView : FieldView {

        public InputField input;

        protected override Task Setup(string fieldName, string fullPath) {
            input.interactable = field.readOnly != true;
            SetupForContentType(input, field.contentType);
            if (!field.regex.IsNullOrEmpty()) { SetupRegexValidator(); }
            return Task.FromResult(true);
        }

        private void SetupRegexValidator() {
            var regexValidator = GetComponent<RegexValidator>();
            if (regexValidator?.errorText != null) {
                var errorText = $"Invalid {field.title}!";
                if (!field.description.IsNullOrEmpty()) { errorText += " Valid: " + field.description; }
                regexValidator.errorText.textLocalized(errorText);
            }
            regexValidator?.EnforceRegex(field.regex);
        }

        private static void SetupForContentType(InputField self, string fieldContentType) {
            if (EnumUtil.TryParse(fieldContentType, out ContentType contentType)) {
                switch (contentType) {
                    case ContentType.Alphanumeric:
                        self.contentType = InputField.ContentType.Alphanumeric;
                        break;
                    case ContentType.Name:
                        self.contentType = InputField.ContentType.Name;
                        break;
                    case ContentType.Email:
                        self.contentType = InputField.ContentType.EmailAddress;
                        break;
                    case ContentType.Password:
                        self.contentType = InputField.ContentType.Password;
                        break;
                    case ContentType.Pin:
                        self.contentType = InputField.ContentType.Pin;
                        break;
                    case ContentType.Essay:
                        self.contentType = InputField.ContentType.Autocorrected;
                        self.lineType = InputField.LineType.MultiLineNewline;
                        ForceRecalculateNeededHeightOfInputField(self);
                        break;
                }
            }
        }

        private static void ForceRecalculateNeededHeightOfInputField(InputField i) {
            i.onValueChanged.AddListener(_ => { LayoutRebuilder.MarkLayoutForRebuild(i.GetComponent<RectTransform>()); });
        }

    }

}