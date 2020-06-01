using com.csutil.model.mtvmtv;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class InputFieldView : FieldView {

        public InputField input;

        protected override Task Setup(string fieldName, string fullPath) {
            input.interactable = field.readOnly != true;
            SetupForContentType(input, field);
            if (field.mandatory == true || !field.pattern.IsNullOrEmpty()) {
                SetupRegexValidator();
            }
            return Task.FromResult(true);
        }

        private void SetupRegexValidator() {
            var regexValidator = GetComponent<RegexValidator>();
            if (regexValidator == null) {
                Log.w($"Can't enforce regex, no validator found in FieldView {this}", gameObject);
                return;
            }
            if (regexValidator.errorText != null) {
                var errorText = $"Invalid {field.title}!";
                if (!field.description.IsNullOrEmpty()) { errorText += " Valid: " + field.description; }
                regexValidator.errorText.textLocalized(errorText);
            }
            regexValidator.isInputRequired = field.mandatory == true;
            regexValidator.EnforceRegex(field.pattern);
        }

        private static void SetupForContentType(InputField self, ViewModel field) {
            if (EnumUtil.TryParse(field.contentType, out ContentType contentType)) {
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
                    default:
                        Log.e($"Content type ignored for field {field.title}: {field.contentType}");
                        break;
                }
            }
        }

        private static void ForceRecalculateNeededHeightOfInputField(InputField i) {
            i.onValueChanged.AddListener(_ => { LayoutRebuilder.MarkLayoutForRebuild(i.GetComponent<RectTransform>()); });
        }

    }

}