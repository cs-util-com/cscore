using com.csutil.model.jsonschema;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.jsonschema {

    public class InputFieldView : FieldView {

        public InputField input;

        protected override Task Setup(string fieldName, string fullPath) {
            input.interactable = field.readOnly != true;
            SetupForContentType(input, field);
            if (field.mandatory == true || !field.pattern.IsNullOrEmpty() || field.minLength != null) {
                SetupRegexValidator();
            }
            if (field.maxLength != null) { input.characterLimit = field.maxLength.Value; }
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
            regexValidator.isInputRequired = field.mandatory == true || field.minLength != null;
            if (field.minLength != null) { regexValidator.minLength = field.minLength.Value; }
            regexValidator.EnforceRegex(field.pattern);
        }

        private static void SetupForContentType(InputField self, JsonSchema field) {
            if (EnumUtil.TryParse(field.format, out ContentFormat contentType)) {
                switch (contentType) {
                    case ContentFormat.alphanumeric:
                        self.contentType = InputField.ContentType.Alphanumeric;
                        break;
                    case ContentFormat.name:
                        self.contentType = InputField.ContentType.Name;
                        break;
                    case ContentFormat.email:
                        self.contentType = InputField.ContentType.EmailAddress;
                        break;
                    case ContentFormat.password:
                        self.contentType = InputField.ContentType.Password;
                        break;
                    case ContentFormat.pin:
                        self.contentType = InputField.ContentType.Pin;
                        break;
                    case ContentFormat.essay:
                        self.contentType = InputField.ContentType.Autocorrected;
                        self.lineType = InputField.LineType.MultiLineNewline;
                        ForceRecalculateNeededHeightOfInputField(self);
                        break;
                    default:
                        Log.e($"Content type ignored for field {field.title}: {field.format}");
                        break;
                }
            }
        }

        private static void ForceRecalculateNeededHeightOfInputField(InputField i) {
            i.onValueChanged.AddListener(_ => { LayoutRebuilder.MarkLayoutForRebuild(i.GetComponent<RectTransform>()); });
        }

    }

}