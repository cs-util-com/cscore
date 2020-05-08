using com.csutil.model.mtvmtv;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class StringFieldView : FieldView {

        public InputField input;

        protected override Task Setup(string fieldName) {
            input.interactable = field.readOnly != true;
            SetupForContentType(input, field.contentType);
            if (!field.regex.IsNullOrEmpty()) { SetupRegexEnforcer(); }
            return Task.FromResult(true);
        }

        private void SetupRegexEnforcer() {
            var regexEnforcer = GetComponent<RegexValidator>();
            if (regexEnforcer?.errorText != null) {
                var errorText = $"Invalid {field.text.name}!";
                if (!field.text.descr.IsNullOrEmpty()) { errorText += " Valid: " + field.text.descr; }
                regexEnforcer.errorText.textLocalized(errorText);
            }
            regexEnforcer?.EnforceRegex(field.regex);
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