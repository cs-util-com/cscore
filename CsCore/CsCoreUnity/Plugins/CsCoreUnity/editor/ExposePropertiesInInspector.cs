using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace com.csutil.editor {

    /// <summary> A custom inspector that allows showing properties in the inspector if they use the [ShowInInspector] attribute </summary>
    internal class PropertyInspectorUi {
        // Modified version of http://wiki.unity3d.com/index.php/Expose_properties_in_inspector

        internal static PropertyInspectorUi[] GetAllProperties(System.Object obj) {
            if (obj == null) { return null; }
            List<PropertyInspectorUi> fields = new List<PropertyInspectorUi>();
            PropertyInfo[] infos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo info in infos) {
                if (!(info.CanRead && info.CanWrite)) { continue; }
                object[] attributes = info.GetCustomAttributes(true);
                bool isExposed = false;
                foreach (object o in attributes) {
                    if (o.GetType() == typeof(ShowInInspectorAttribute)) {
                        isExposed = true;
                        break;
                    }
                }
                if (!isExposed) { continue; }
                SerializedPropertyType type = SerializedPropertyType.Integer;
                if (PropertyInspectorUi.GetPropertyType(info, out type)) {
                    PropertyInspectorUi field = new PropertyInspectorUi(obj, info, type);
                    fields.Add(field);
                }
            }
            return fields.ToArray();
        }

        internal static void DrawPropertiesInInspector(PropertyInspectorUi[] properties) {
            if (properties == null || properties.Length == 0) { return; }
            GUILayoutOption[] emptyOptions = new GUILayoutOption[0];
            EditorGUILayout.BeginVertical(emptyOptions);
            foreach (PropertyInspectorUi field in properties) {
                EditorGUILayout.BeginHorizontal(emptyOptions);
                switch (field.Type) {
                    case SerializedPropertyType.Integer:
                        field.SetValue(EditorGUILayout.IntField(field.Name, (int)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.Float:
                        field.SetValue(EditorGUILayout.FloatField(field.Name, (float)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.Boolean:
                        field.SetValue(EditorGUILayout.Toggle(field.Name, (bool)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.String:
                        field.SetValue(EditorGUILayout.TextField(field.Name, (String)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.Vector2:
                        field.SetValue(EditorGUILayout.Vector2Field(field.Name, (Vector2)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.Vector3:
                        field.SetValue(EditorGUILayout.Vector3Field(field.Name, (Vector3)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.Enum:
                        field.SetValue(EditorGUILayout.EnumPopup(field.Name, (Enum)field.GetValue(), emptyOptions));
                        break;
                    case SerializedPropertyType.ObjectReference:
                        field.SetValue(EditorGUILayout.ObjectField(field.Name, (UnityEngine.Object)field.GetValue(), field.GetPropertyType(), true, emptyOptions));
                        break;
                    default:
                        break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private System.Object m_Instance;
        private PropertyInfo m_Info;
        private SerializedPropertyType m_Type;
        private MethodInfo m_Getter;
        private MethodInfo m_Setter;

        private SerializedPropertyType Type { get { return m_Type; } }

        private string Name { get { return ObjectNames.NicifyVariableName(m_Info.Name); } }

        private PropertyInspectorUi(System.Object instance, PropertyInfo info, SerializedPropertyType type) {
            m_Instance = instance;
            m_Info = info;
            m_Type = type;
            m_Getter = m_Info.GetGetMethod();
            m_Setter = m_Info.GetSetMethod();
        }

        private System.Object GetValue() { return m_Getter.Invoke(m_Instance, null); }

        private void SetValue(System.Object value) {
            if (m_Setter == null) { return; }
            m_Setter.Invoke(m_Instance, new System.Object[] { value });
        }

        public Type GetPropertyType() { return m_Info.PropertyType; }

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