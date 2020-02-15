using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace com.csutil.editor {

    /// <summary> 
    /// A custom inspector that automatically shows all public properties in the inspector 
    /// If the compile constant ENABLE_CSUTIL_PROPERTY_MAGIC is NOT set to true manually
    /// a property must have the [ShowInInspector] annotation to show in the Unity inspector
    /// </summary>
    internal class ShowPropertiesInInspector {
        // Initial idea from http://wiki.unity3d.com/index.php/Expose_properties_in_inspector

        internal static ShowPropertiesInInspector[] GetPropertiesToDraw(System.Object obj) {
            if (obj == null) { return null; }
            // Skip all UnityEngine classes:
            var nameSpace = obj.GetType().Namespace;
            if (nameSpace != null && nameSpace.StartsWith("UnityEngine")) { return null; }

            var readableProps = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(p => p.CanRead);

            var propertyInspectorUis = new List<ShowPropertiesInInspector>();
            foreach (PropertyInfo property in readableProps) {
                var type = SerializedPropertyType.Integer;
                if (GetPropertyType(property, out type)) {
                    if (property.HasAttribute<HideInInspector>(true)) { continue; }
#if !ENABLE_CSUTIL_PROPERTY_MAGIC
                    if (!property.HasAttribute<ShowPropertyInInspector>()) { continue; }
#endif
                    propertyInspectorUis.Add(new ShowPropertiesInInspector(obj, property, type));
                }
            }
            return propertyInspectorUis.ToArray();
        }

        internal static void DrawInInspector(ShowPropertiesInInspector[] properties) {
            if (properties == null || properties.Length == 0) { return; }
            GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
            EditorGUILayout.BeginVertical(emptyOptions);
            foreach (ShowPropertiesInInspector property in properties) { property.DrawInInspector(emptyOptions); }
            EditorGUILayout.EndVertical();
        }

        private System.Object objInstance;
        private PropertyInfo property;
        private SerializedPropertyType propertyType;
        private MethodInfo propertyGet;
        private MethodInfo propertySet;
        private string propertyName;

        private ShowPropertiesInInspector(System.Object objectInstance, PropertyInfo prop, SerializedPropertyType propertyType) {
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

        private void DrawInInspector(GUILayoutOption[] o) {
            EditorGUILayout.BeginHorizontal(o);
            GUI.enabled = PropertyHasASetter();
            try { DrawEditorLayoutUi(o); } catch (Exception e) { Log.w("Draw error for property " + propertyName + ": " + e); }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEditorLayoutUi(GUILayoutOption[] o) {
            switch (propertyType) {
                case SerializedPropertyType.Integer:
                    SetValue(EditorGUILayout.IntField(GetLabelForProp(), (int)GetValue(), o));
                    break;
                case SerializedPropertyType.Float:
                    SetValue(EditorGUILayout.FloatField(GetLabelForProp(), (float)GetValue(), o));
                    break;
                case SerializedPropertyType.Boolean:
                    SetValue(EditorGUILayout.Toggle(GetLabelForProp(), (bool)GetValue(), o));
                    break;
                case SerializedPropertyType.String:
                    SetValue(EditorGUILayout.TextField(GetLabelForProp(), (String)GetValue(), o));
                    break;
                case SerializedPropertyType.Vector2:
                    SetValue(EditorGUILayout.Vector2Field(GetLabelForProp(), (Vector2)GetValue(), o));
                    break;
                case SerializedPropertyType.Vector3:
                    SetValue(EditorGUILayout.Vector3Field(GetLabelForProp(), (Vector3)GetValue(), o));
                    break;
                case SerializedPropertyType.Enum:
                    SetValue(EditorGUILayout.EnumPopup(GetLabelForProp(), (Enum)GetValue(), o));
                    break;
                case SerializedPropertyType.ObjectReference:
                    var v = GetValue() as UnityEngine.Object;
                    try {
                        SetValue(EditorGUILayout.ObjectField(GetLabelForProp(), v, GetPropertyType(), true, o));
                    } catch { }
                    break;
                default:
                    break;
            }
        }

        private string GetLabelForProp() { return "Prop.:" + propertyName; }
    }

}