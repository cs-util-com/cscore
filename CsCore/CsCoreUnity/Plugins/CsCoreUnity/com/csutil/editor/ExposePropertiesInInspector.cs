using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace com.csutil.editor {

    /// <summary> A custom inspector that allows showing properties in the inspector if they use the [ShowInInspector] attribute </summary>
    internal class PropertyInspectorUi {
        // Modified version of http://wiki.unity3d.com/index.php/Expose_properties_in_inspector

        internal static PropertyInspectorUi[] GetAllProperties(System.Object obj) {
            if (obj == null) { return null; }
            var readableProps = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(p => p.CanRead);

            var propertyInspectorUis = new List<PropertyInspectorUi>();
            foreach (PropertyInfo property in readableProps) {
                var type = SerializedPropertyType.Integer;
                if (GetPropertyType(property, out type)) { propertyInspectorUis.Add(new PropertyInspectorUi(obj, property, type)); }
            }
            return propertyInspectorUis.ToArray();
        }

        internal static void DrawPropertiesInInspector(PropertyInspectorUi[] properties) {
            if (properties == null || properties.Length == 0) { return; }
            GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
            EditorGUILayout.BeginVertical(emptyOptions);
            foreach (PropertyInspectorUi property in properties) {
                EditorGUILayout.BeginHorizontal(emptyOptions);
                GUI.enabled = property.PropertyHasASetter();
                ShowEditorInspectorUiForField(property, emptyOptions);
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private static void ShowEditorInspectorUiForField(PropertyInspectorUi prop, GUILayoutOption[] o) {
            switch (prop.propertyType) {
                case SerializedPropertyType.Integer:
                    prop.SetValue(EditorGUILayout.IntField(prop.propertyName, (int)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.Float:
                    prop.SetValue(EditorGUILayout.FloatField(prop.propertyName, (float)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.Boolean:
                    prop.SetValue(EditorGUILayout.Toggle(prop.propertyName, (bool)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.String:
                    prop.SetValue(EditorGUILayout.TextField(prop.propertyName, (String)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.Vector2:
                    prop.SetValue(EditorGUILayout.Vector2Field(prop.propertyName, (Vector2)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.Vector3:
                    prop.SetValue(EditorGUILayout.Vector3Field(prop.propertyName, (Vector3)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.Enum:
                    prop.SetValue(EditorGUILayout.EnumPopup(prop.propertyName, (Enum)prop.GetValue(), o));
                    break;
                case SerializedPropertyType.ObjectReference:
                    prop.SetValue(EditorGUILayout.ObjectField(prop.propertyName, (UnityEngine.Object)prop.GetValue(), prop.GetPropertyType(), true, o));
                    break;
                default:
                    break;
            }
        }

        private System.Object objInstance;
        private PropertyInfo property;
        private SerializedPropertyType propertyType;
        private MethodInfo propertyGet;
        private MethodInfo propertySet;
        private string propertyName;

        private PropertyInspectorUi(System.Object objectInstance, PropertyInfo prop, SerializedPropertyType propertyType) {
            this.objInstance = objectInstance;
            this.property = prop;
            this.propertyType = propertyType;
            this.propertyName = ObjectNames.NicifyVariableName(property.Name);
            this.propertyGet = property.GetGetMethod();
            this.propertySet = property.GetSetMethod();
        }

        private System.Object GetValue() { return propertyGet.Invoke(objInstance, null); }

        private void SetValue(System.Object value) {
            if (!PropertyHasASetter()) { return; }
            propertySet.Invoke(objInstance, new System.Object[] { value });
        }

        private bool PropertyHasASetter() { return propertySet != null; }

        public Type GetPropertyType() { return property.PropertyType; }

        private static bool GetPropertyType(PropertyInfo info, out SerializedPropertyType propertyType) {
            propertyType = SerializedPropertyType.Generic;
            Type type = info.PropertyType;
            if (type == typeof(int)) {
                propertyType = SerializedPropertyType.Integer;
                return true;
            }
            if (type == typeof(float)) {
                propertyType = SerializedPropertyType.Float;
                return true;
            }
            if (type == typeof(bool)) {
                propertyType = SerializedPropertyType.Boolean;
                return true;
            }
            if (type == typeof(string)) {
                propertyType = SerializedPropertyType.String;
                return true;
            }
            if (type == typeof(Vector2)) {
                propertyType = SerializedPropertyType.Vector2;
                return true;
            }
            if (type == typeof(Vector3)) {
                propertyType = SerializedPropertyType.Vector3;
                return true;
            }
            if (type.IsEnum) {
                propertyType = SerializedPropertyType.Enum;
                return true;
            }
            // COMMENT OUT to NOT expose custom objects/types
            propertyType = SerializedPropertyType.ObjectReference;
            return true;
        }

    }

}