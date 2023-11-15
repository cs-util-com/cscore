using com.csutil.io;
using System;
using System.Collections.Generic;
using System.IO;
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
            // Log.d($"LoadPrefab '{pathInResourcesFolder}'");
            GameObject prefab = LoadV2<GameObject>(pathInResourcesFolder);
            if (prefab == null) { throw new FileNotFoundException("Could not find prefab in any /Resources/.. folder under path='../" + pathInResourcesFolder + "'"); }
            var prefabInstance = InstantiatePrefab(prefab, keepReferenceToEditorPrefab);
            prefabInstance.name = pathInResourcesFolder;
            EventBus.instance.Publish(EventConsts.catTemplate, prefabInstance);
            return prefabInstance;
        }

        public static GameObject InstantiatePrefab(GameObject prefab, bool keepReferenceToEditorPrefab = false) {
 #if UNITY_EDITOR
            if (keepReferenceToEditorPrefab) { return UnityEditor.PrefabUtility.InstantiatePrefab(prefab) as GameObject; }
#endif
            return GameObject.Instantiate(prefab);
        }

        /// <summary> Force AssetDB entry reload by Unity, otherwise Resources files are cached by it </summary>
        [System.Diagnostics.Conditional("DEBUG")] // To remove line in builts
        public static void ForceAssetDatabaseReimport(string pathInResources) {
#if UNITY_EDITOR 
            if (Application.isPlaying) { return; }
            // Only way that I found to get the full path of assets is to load it and ask what its path is:
            var pathInAssets = UnityEditor.AssetDatabase.GetAssetPath(Resources.Load(pathInResources));
            UnityEditor.AssetDatabase.ImportAsset(pathInAssets, UnityEditor.ImportAssetOptions.ForceUpdate);
#endif
        }

        /// <summary> Loads an asset from the Resources of the Unity project </summary>
        /// <param name="pathInResourcesFolder"> a path like Colors/colorScheme1 in a Resource folder of 
        /// your project and WITHOUT a file extension </param>
        /// <param name="forceAssetDbReimport"> If true will force the unity AssetDatabase to reload 
        /// the asset if its already cached, only relevant in Editor, ignored in runtime </param>
        /// <returns></returns>
        public static T LoadV2<T>(string pathInResourcesFolder, bool forceAssetDbReimport = false) {
            if (pathInResourcesFolder.IsNullOrEmpty()) {
                throw new ArgumentNullException("pathInResourcesFolder null or emtpy");
            }
            if (forceAssetDbReimport) { ForceAssetDatabaseReimport(pathInResourcesFolder); }
            pathInResourcesFolder = RemovePathPrefixIfNeeded(pathInResourcesFolder);
            pathInResourcesFolder = RemoveExtensionIfNeeded(pathInResourcesFolder, ".prefab");
            pathInResourcesFolder = RemoveExtensionIfNeeded(pathInResourcesFolder, ".asset");
            if ((typeof(T).IsCastableTo<string>())) {
                TextAsset textAsset = LoadV2<TextAsset>(pathInResourcesFolder, forceAssetDbReimport);
                if (textAsset == null) { throw new FileNotFoundException("No text asset found at " + pathInResourcesFolder); }
                return (T)(object)textAsset.text;
            }
            if ((typeof(MemoryStream).IsCastableTo<T>())) {
                TextAsset textAsset = LoadV2<TextAsset>(pathInResourcesFolder, forceAssetDbReimport);
                if (textAsset == null) { throw new FileNotFoundException("No text asset found at " + pathInResourcesFolder); }
                return (T)(object)new MemoryStream(textAsset.bytes); 
            }
            if (ResourceCache.TryLoad(pathInResourcesFolder, out T result)) { return result; }
            return (T)(object)Resources.Load(pathInResourcesFolder, typeof(T));
        }

        private static string RemovePathPrefixIfNeeded(string pathInResourcesFolder) {
            if (pathInResourcesFolder.StartsWith("/")) { return pathInResourcesFolder.Substring(1); }
            return pathInResourcesFolder;
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
            return Resources.FindObjectsOfTypeAll<T>().Filter(comp => comp.gameObject.IsInActiveScene());
        }

        public static IEnumerable<GameObject> FindAllGOsInScene() {
            return Resources.FindObjectsOfTypeAll<GameObject>().Filter(IsInActiveScene);
        }

        private static bool IsInActiveScene(this GameObject go) {
            return go.activeInHierarchy || !go.IsPartOfEditorOnlyPrefab();
        }

        // See https://answers.unity.com/answers/1190932/view.html
        public static bool IsPartOfEditorOnlyPrefab(this GameObject go) {
            return go.scene.rootCount == 0 || go.scene.name == null;
        }

    }

}
