using com.csutil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Plugins.CsCoreUnity.com.csutil.editor {

    public class EditorSettingsExtensions {

        public static bool switchSerializationToForceText() {
#if UNITY_EDITOR
            if (UnityEditor.EditorSettings.serializationMode != UnityEditor.SerializationMode.ForceText) {
                Log.e("Switching Scene and Prefab serialization mode to 'ForceText'");
                UnityEditor.EditorSettings.serializationMode = UnityEditor.SerializationMode.ForceText;
                return true;
            }
#endif
            return false;
        }

    }

}
