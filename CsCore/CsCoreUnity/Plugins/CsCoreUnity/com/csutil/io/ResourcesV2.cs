using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {
    public static class ResourcesV2 {

        /// <summary> 
        /// Load prefab from a Resources subpath, for example:
        ///      MyUi1.prefab is located in Assets/Ui/Resources/Ui1 
        ///   -> The path must be "Ui1/MyUi1"
        /// </summary>
        /// <param name="pathInResourcesFolder"></param>
        /// <param name="keepReferenceToEditorPrefab"> Set to true if prefab loaded by editor script </param>
        public static GameObject LoadPrefab(string pathInResourcesFolder, bool keepReferenceToEditorPrefab = false) {
            GameObject prefab = LoadV2<GameObject>(pathInResourcesFolder);
            if (prefab == null) { throw new Exception("could not load prefab from path=" + pathInResourcesFolder); }
#if UNITY_EDITOR
            if (keepReferenceToEditorPrefab) { return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject; }
#endif
            var prefabInstance = GameObject.Instantiate(prefab) as GameObject;
            prefabInstance.name = "Prefab:" + pathInResourcesFolder;
            return prefabInstance;
        }

        public static T LoadV2<T>(string path) { return (T)(object)Resources.Load(path, typeof(T)); }

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
