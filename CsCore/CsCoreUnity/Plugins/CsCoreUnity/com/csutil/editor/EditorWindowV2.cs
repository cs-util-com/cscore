using System;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    public class EditorWindowV2 : EditorWindow {

        public static T ShowUtilityWindow<T>(Action<T> onGUI, int size = 250) where T : EditorWindowV2 {
            T uiContainer = CreateInstance<T>();
            uiContainer.onGUI = delegate { onGUI(uiContainer); };
            uiContainer.position = new Rect(Screen.width / 2, Screen.height / 2, size, size);
            uiContainer.ShowUtility();
            return uiContainer;
        }

        public Action onGUI;
        void OnGUI() { onGUI(); }

    }

}
