using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zio;

namespace com.csutil.ui {

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Theme))]
    public class ThemeUnityEditorUi : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            if (GUILayout.Button("Save current colors to JSON")) {
                var path = UnityEditor.EditorUtility.SaveFilePanel("Save current colors to JSON", "", "", "json");
                var fileToSaveTo = new FileInfo(path).ToFileEntryInNewRoot();
                (target as Theme)?.SaveToSchemeJson(fileToSaveTo);
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

        public bool TryGetColor(string colorName, out Color c) {
            c = Color.clear;
            if (colors.IsNullOrEmpty()) { return false; }
            AssertV2.IsNotNull(colorName, "colorName");
            AssertV2.IsFalse(colors.IsNullOrEmpty(), "colors.IsNullOrEmpty");
            var namedColor = colors.FirstOrDefault(x => x.colorName == colorName);
            if (namedColor != null) { c = namedColor.colorValue; return true; }
            return false;
        }

        private void OnEnable() {
            InitColorsIfEmpty();
            this.ExecuteRepeated(() => { CheckIfColorsChanged(); return true; }, 1000);
        }

        private void InitColorsIfEmpty() {
            if (colors.IsNullOrEmpty()) { colors = LoadHexColors(schemeName).Map(ToNamedColor).ToList(); }
        }

        private static NamedColor ToNamedColor(KeyValuePair<string, string> hexColor) {
            var c = ColorUtil.HexStringToColor(hexColor.Value);
            return new NamedColor() { colorName = hexColor.Key, colorValue = c };
        }

        private static Dictionary<string, string> LoadHexColors(string schemeName) {
            var themeColorsJson = ResourcesV2.LoadV2<string>(schemeName, forceAssetDbReimport: true);
            return JsonReader.GetReader().Read<Dictionary<string, string>>(themeColorsJson);
        }

        public void SaveToSchemeJson(FileEntry fileToSaveTo) {
            fileToSaveTo.SaveAsText(JsonWriter.AsPrettyString(ToHexColors(colors)));
        }

        private static Dictionary<string, string> ToHexColors(List<NamedColor> colors) {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var c in colors) {
                result.Add(c.colorName, ColorUtility.ToHtmlStringRGBA(c.colorValue));
            }
            return result;
        }

        private void OnValidate() {
            if (ApplicationV2.IsEditorOnValidateAllowed()) {
                InitColorsIfEmpty();
                CheckIfColorsChanged();
            }
        }

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
            var allAffected = ResourcesV2.FindAllInScene<ThemeColor>().Filter(x => x._colorName == c.colorName);
            foreach (var themeColor in allAffected) { themeColor.ApplyColor(c.colorValue); }
        }

    }

}