using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace com.csutil.ui {

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Theme))]
    public class ThemeUnityEditorUi : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save current colors to JSON")) {
                var path = UnityEditor.EditorUtility.SaveFilePanel("Save current colors to JSON", "", "", "json");
                (target as Theme)?.SaveToSchemeJson(path);
            }
        }
    }
#endif

    public class Theme : MonoBehaviour {

        [Serializable]
        public class NamedColor {
            public string colorName;
            public Color colorValue;
        }

        public string schemeName = "Colors/colorScheme1";
        public List<NamedColor> colors = new List<NamedColor>();
        private List<NamedColor> oldColors = new List<NamedColor>();

        public void ApplyTheme(string colorName, ThemeColor target) {
            if (TryGetColor(colorName, out Color color)) { ApplyColor(target, color); }
        }

        public bool TryGetColor(string colorName, out Color c) {
            c = Color.clear;
            if (colors.IsNullOrEmpty()) { return false; }
            var namedColor = colors.FirstOrDefault(x => x.colorName == colorName);
            if (namedColor != null) { c = namedColor.colorValue; return true; }
            Log.w("Color not found in colors: " + colorName);
            return false;
        }

        private void Start() {
            if (colors.IsNullOrEmpty()) { colors = LoadHexColors(schemeName).Map(ToNamedColor).ToList(); }
            this.ExecuteRepeated(() => { CheckIfColorsChanged(); return true; }, 1000);
        }

        private NamedColor ToNamedColor(KeyValuePair<string, string> hexColor) {
            if (ColorUtility.TryParseHtmlString(hexColor.Value, out Color c)) {
                return new NamedColor() { colorName = hexColor.Key, colorValue = c };
            }
            Log.w("Could not parse hex color value: " + hexColor.Value);
            return null;
        }

        private static Dictionary<string, string> LoadHexColors(string themeName) {
            var themeColorsJson = ResourcesV2.LoadV2<string>(themeName);
            return JsonReader.GetReader().Read<Dictionary<string, string>>(themeColorsJson);
        }

        public void SaveToSchemeJson(string pathToSaveTo) {
            new FileInfo(pathToSaveTo).SaveAsText(JsonWriter.AsPrettyString(ToHexColors(colors)));
        }

        private static Dictionary<string, string> ToHexColors(List<NamedColor> colors) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var c in colors) {
                result.Add(c.colorName, ColorUtility.ToHtmlStringRGBA(c.colorValue));
            }
            return result;
        }

        private static void ApplyColor(ThemeColor target, Color color) {
            var graphic = target.GetComponentV2<Graphic>();
            if (graphic == null) { Log.w("Passed target graphic was null!"); return; }
            graphic.color = color;
        }

        private void OnValidate() { CheckIfColorsChanged(); }

        private void CheckIfColorsChanged() {
            for (int i = 0; i < colors.Count; i++) {
                if (oldColors.Count <= i || !EqualJson(colors[i], oldColors[i])) {
                    UpdateThemeColorMonos(colors[i]);
                }
            }
            oldColors = colors.Map(x => JsonUtility.FromJson<NamedColor>(JsonUtility.ToJson(x))).ToList();
        }

        private bool EqualJson<T>(T a, T b) { return JsonUtility.ToJson(a) == JsonUtility.ToJson(b); }

        private static void UpdateThemeColorMonos(NamedColor c) {
            var allAffected = ResourcesV2.FindAllInScene<ThemeColor>().Filter(x => x.colorName == c.colorName);
            foreach (var mono in allAffected) { ApplyColor(mono, c.colorValue); }
        }

    }

}