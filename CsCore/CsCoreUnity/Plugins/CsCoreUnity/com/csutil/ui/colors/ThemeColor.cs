using UnityEngine;
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
            element, elementContrast, elementContrastWeak,
            button, buttonContrast, buttonContrastWeak
        }

        public string _colorName;

        [ShowPropertyInInspector]
        public ColorNames colorNameSuggestion {
            get { return ColorNames.custom.TryParse(_colorName); }
            set { if (ColorNames.custom != value) { SetColor(value.GetEntryName()); } }
        }

        private void SetColor(string newColor) {
            _colorName = newColor;
            Refresh();
        }

        public void Refresh() {
            if (_colorName.IsNullOrEmpty()) { return; }
            var theme = IoC.inject.GetOrAddComponentSingleton<Theme>(this);
            if (theme.TryGetColor(_colorName, out Color color)) { ApplyColor(color); }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnEnable() { Refresh(); } // also needed to be able to disable in Editor

        private void OnValidate() { if (ApplicationV2.IsUnityFullyInitialized()) { Refresh(); } }

        public void ApplyColor(Color color) {
            if (!enabled) { return; }
            var graphic = this.GetComponentV2<Graphic>();
            if (graphic != null) { graphic.color = color; return; }
            var s = this.GetComponentV2<Selectable>();
            if (s != null && s.targetGraphic != null) { s.targetGraphic.color = color; return; }
            Log.e("Could not find anything to apply the ThemeColor to!");
        }

    }

}