using UnityEngine;
using UnityEditor;

namespace com.csutil.editor {

    [CustomEditor(typeof(MonoBehaviour), true)]
    public class DefaultMonoInspectorV2 : Editor {
        private ShowPropertiesInInspector[] propsToShowInInspector;

        public void OnEnable() {
            propsToShowInInspector = ShowPropertiesInInspector.GetPropertiesToDraw(target);
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            ShowPropertiesInInspector.DrawInInspector(propsToShowInInspector);
        }

    }

}