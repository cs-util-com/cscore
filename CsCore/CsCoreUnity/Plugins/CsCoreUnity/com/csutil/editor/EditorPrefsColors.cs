using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    public class EditorPrefsColors {

        // try any combination of static/instance and public/private:
        private const BindingFlags anyBindings = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static void SetPlaymodeTintColor(Color c) { SetColor(c, "Playmode tint"); }
        public static void SetSceneBackgroundColor(Color c) { SetColor(c, "Scene/Background"); }

        private static void SetColor(Color newColorValue, string colorName) {
            var PrefSettings = GetEditorClasses("PrefSettings").First();
            var m_Prefs = PrefSettings.GetField("m_Prefs", anyBindings);
            var colorPref = GetPref(m_Prefs, colorName);

            var PrefColorTypes = GetEditorClasses("PrefColor");
            var PrefColor = PrefColorTypes.First();
            var m_Color = PrefColor.GetField("m_Color", anyBindings);
            m_Color.SetValue(colorPref, newColorValue);

            var ToUniqueString = PrefColor.GetMethod("ToUniqueString", anyBindings);
            EditorPrefs.SetString(colorName, (string)ToUniqueString.Invoke(colorPref, null));
        }

        private static object GetPref(FieldInfo m_PrefsField, string aName) {
            var prefList = (SortedList<string, object>)m_PrefsField.GetValue(null);
            return prefList[aName];
        }

        private static List<Type> GetEditorClasses(string aName) {
            return typeof(Editor).Assembly.GetTypes().Where((a) => a.Name.Contains(aName)).ToList();
        }

    }

}
