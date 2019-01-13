using UnityEngine;
using UnityEditor;

namespace com.csutil.editor {

    [CustomEditor(typeof(MonoBehaviour), true)]
    public class DefaultMonoInspectorV2 : Editor {
        private PropertyInspectorUi[] propertiesMarkedAsShowInInspector;

        public void OnEnable() {
            propertiesMarkedAsShowInInspector = PropertyInspectorUi.GetAllProperties(target);
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            PropertyInspectorUi.DrawPropertiesInInspector(propertiesMarkedAsShowInInspector);
        }

    }

}