using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    /// <summary> This processor warns about any classes that dont have a namespace specified </summary>
    class MissingNamespaceFinder : AssetPostprocessor {

        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReloadScripts() {
            var allAs = GetAllAssembliesInProject();
            allAs = allAs.Filter(x => !x.FullName.StartsWith("UnityEngine")); // Exclude all Unity assemblies
            foreach (var assembly in allAs) { CheckTypesInAssembly(assembly); }
        }

        private static IEnumerable<Assembly> GetAllAssembliesInProject() {
            return GameObject.FindObjectsOfType<MonoBehaviour>().Map(x => x.GetType().Assembly).Distinct();
        }

        private static void CheckTypesInAssembly(Assembly assembly) {
            var types = assembly.GetTypesWithMissingNamespace();
            foreach (var type in types) { Debug.LogError("Found a class without a namespace: " + type); }
        }

    }

}
