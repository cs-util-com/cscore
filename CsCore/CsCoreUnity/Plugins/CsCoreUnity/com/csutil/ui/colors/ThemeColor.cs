using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.csutil.ui {

    public class ThemeColor : MonoBehaviour {

        public enum ColorNames {
            custom,
            accent, accentContrast, accentContrastWeak,
            primary, primaryContrast, primaryContrastWeak,
            background, backgroundContrast, backgroundContrastWeak,
            card, cardDark, cardContrast, cardContrastWeak,
            transparent, transparentContrast, transparentContrastWeak,
            shadow, shadowContrast, shadowContrastWeak,
            warning, warningContrast, warningContrastWeak,
            element, elementLight, elementContrast, elementContrastWeak,
            button, buttonContrast, buttonContrastWeak
        }

        public string _colorName;
        public SetColorEvent applyColorToTarget = new SetColorEvent();

        [ShowPropertyInInspector]
        public ColorNames colorNameSuggestion {
            get { return ColorNames.custom.TryParse(_colorName); }
            set { if (ColorNames.custom != value) { SetColor(value.GetEntryName()); } }
        }

        [System.Serializable]
        public class SetColorEvent : UnityEvent<Color> { }

        private void OnEnable() { Refresh(); }

        private void SetColor(string newColor) {
            _colorName = newColor;
            Refresh();
        }

        private void OnValidate() { if (ApplicationV2.IsEditorOnValidateAllowed()) { Refresh(); } }

        public void Refresh() {
            if (_colorName.IsNullOrEmpty()) { return; }
            var theme = IoC.inject.GetOrAddComponentSingleton<Theme>(this);
            if (theme.TryGetColor(_colorName, out Color color)) { ApplyColor(color); }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void ApplyColor(Color color) {
            if (!enabled) { return; }
            if (applyColorToTarget.IsNullOrEmpty()) {
                var rectagle = this.GetComponentV2<Rectangle>();
                if (rectagle != null) { SetRectangleColor(rectagle, color); return; }
                var graphic = this.GetComponentV2<Graphic>();
                if (graphic != null) { SetGraphicColor(graphic, color); return; }
                var sel = this.GetComponentV2<Selectable>();
                if (sel != null && sel.targetGraphic != null) { SetSelectableColor(sel, color); return; }
                Log.e("Could not find anything to apply the ThemeColor to!", gameObject);
                enabled = false;
            } else {
                applyColorToTarget.Invoke(color);
            }
        }

        private static void SetSelectableColor(Selectable self, Color color) {
            if (self.targetGraphic.color != color) { self.targetGraphic.color = color; }
        }

        private static void SetGraphicColor(Graphic self, Color color) {
            if (self.color != color) { self.color = color; }
        }

        private static void SetRectangleColor(Rectangle self, Color color) {
            if (self.color != color) {
                self.ShapeProperties.OutlineColor = color;
                self.color = color;
            }
        }
    }

}