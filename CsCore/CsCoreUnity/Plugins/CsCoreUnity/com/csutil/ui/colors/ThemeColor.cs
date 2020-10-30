using System.Collections.Generic;
using System.Linq;
using ThisOtherThing.UI.Shapes;
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
            element, elementLight, elementContrast, elementContrastWeak,
            button, buttonContrast, buttonContrastWeak
        }

        public string _colorName;
        public Component target;

        [ShowPropertyInInspector]
        public ColorNames colorNameSuggestion {
            get { return ColorNames.custom.TryParse(_colorName); }
            set { if (ColorNames.custom != value) { SetColor(value.GetEntryName()); } }
        }

        private void OnEnable() { Refresh(); }

        private void SetColor(string newColor) {
            _colorName = newColor;
            Refresh();
        }

        private void OnValidate() { if (ApplicationV2.IsEditorOnValidateAllowed()) { Refresh(); } }

        public void Refresh() {
            if (!this.enabled) { return; }
            if (_colorName.IsNullOrEmpty()) { return; }
            var theme = IoC.inject.GetOrAddComponentSingleton<Theme>(this);
            if (theme.TryGetColor(_colorName, out Color color)) {
                ApplyColor(color);
            } else {
                Log.w($"Color '{_colorName}' not found in theme colors)", gameObject);
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            DisableIfMultipleThemeColorsWithSameTarget();
#endif
        }

        private void DisableIfMultipleThemeColorsWithSameTarget() {
            var list = GetConflictingThemeColors();
            if (!list.IsNullOrEmpty()) {
                Log.e($"{list.Count()} ThemeColors try to modify the same target!", gameObject);
                this.enabled = false;
            }
        }

        private IEnumerable<ThemeColor> GetConflictingThemeColors() {
            return GetComponents<ThemeColor>().Filter(x => x != this && x.target == target && x.isActiveAndEnabled);
        }

        public void ApplyColor(Color color) {
            if (!enabled) { return; }
            if (target == null) { target = LazyInitTarget(); }
            if (target is Rectangle r) { SetRectangleColor(r, color); return; }
            if (target is Graphic g) { SetGraphicColor(g, color); return; }
            if (target is Selectable s && s.targetGraphic != null) { SetSelectableColor(s, color); return; }
            Log.e($"Could not apply the ThemeColor to target='{target}'", gameObject);
            enabled = false;
        }

        private Component LazyInitTarget() {
            if (gameObject.HasComponent(out Rectangle r)) { return r; }
            if (gameObject.HasComponent(out Graphic g)) { return g; }
            if (gameObject.HasComponent(out Selectable s)) { return s; }
            Log.e("Could not automatically find target for ThemeColor", gameObject);
            return null;
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