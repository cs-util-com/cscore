using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    public static class ResourcesV2 {

        /// <summary> 
        /// Load prefab from a Resources subpath, for example:
        ///   MyUi1.prefab is located in Assets/Ui/Resources/Ui1 
        ///   -> The path must be "Ui1/MyUi1.prefab" or "Ui1/MyUi1"
        /// </summary>
        /// <param name="keepReferenceToEditorPrefab"> Set it true if prefab loaded by editor script </param>
        public static GameObject LoadPrefab(string pathInResourcesFolder, bool keepReferenceToEditorPrefab = false) {
            GameObject prefab = LoadV2<GameObject>(pathInResourcesFolder);
            if (prefab == null) { throw new Exception("Could not find prefab at path='" + pathInResourcesFolder + "'"); }
#if UNITY_EDITOR
            if (keepReferenceToEditorPrefab) { return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject; }
#endif
            var prefabInstance = GameObject.Instantiate(prefab) as GameObject;
            prefabInstance.name = "Prefab:" + pathInResourcesFolder;
            EventBus.instance.Publish(IoEvents.PREFAB_LOADED, prefabInstance);
            return prefabInstance;
        }

        public static T LoadV2<T>(string pathInResourcesFolder) {
            pathInResourcesFolder = RemoveExtensionIfNeeded(pathInResourcesFolder, ".prefab");
            pathInResourcesFolder = RemoveExtensionIfNeeded(pathInResourcesFolder, ".asset");
            return (T)(object)Resources.Load(pathInResourcesFolder, typeof(T));
        }

        private static string RemoveExtensionIfNeeded(string path, string fileExtension) {
            if (path.EndsWith(fileExtension)) { return path.Substring(fileExtension, includeEnd: false); }
            return path;
        }

        /// <summary> 
        /// Load a ScriptableObject instance from a Resources subpath, for example:
        ///   MyExampleScriptableObject_Instance1.asset is located in Assets/Ui/Resources/MyFolderX 
        ///   -> The path must be "MyFolderX/MyExampleScriptableObject_Instance1" 
        /// </summary>
        public static T LoadScriptableObjectInstance<T>(string pathInResourcesFolder) where T : ScriptableObject {
            var so = LoadV2<T>(pathInResourcesFolder);
            AssertV2.IsNotNull(so, "ScriptableObject (" + typeof(T) + ") instance " + pathInResourcesFolder);
            return so;
        }

        /// <summary> Returns all comp. in scene (including all inactive) </summary>
        public static IEnumerable<T> FindAllInScene<T>() where T : Component {
            return Resources.FindObjectsOfTypeAll<T>().Filter(comp => !comp.IsPartOfEditorOnlyPrefab());
        }

        // See http://answers.unity.com/answers/1190932/view.html
        private static bool IsPartOfEditorOnlyPrefab(this Component self) {
            return self.gameObject.scene.rootCount == 0 || self.gameObject.scene.name == null;
        }

    }

}
