using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    /// <summary> This processor warns about any classes that dont have a namespace specified </summary>
    public class MissingNamespaceFinder : AssetPostprocessor {

        /// <summary> Add assembly names to this list that should be ignored by the checker </summary>
        public static HashSet<string> blackList = new HashSet<string>() { "UnityEngine" };

        [UnityEditor.Callbacks.DidReloadScripts]
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        static void DidReloadScripts() { CheckAllAssembliesInProject(); }
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        private static async Task CheckAllAssembliesInProject() {
            await TaskV2.Delay(1000);
            var allAssemblies = GetAllAssembliesInProject().Filter(ShouldBeIncludedInCheck);
            foreach (var assembly in allAssemblies) { CheckTypesInAssembly(assembly); }
        }

        /// <summary> Exclude e.g. all Unity assemblies </summary>
        private static bool ShouldBeIncludedInCheck(Assembly x) {
            var name = x.FullName;
            foreach (string assembly in blackList) { if (name.StartsWith(assembly)) { return false; } }
            return true;
        }

        private static IEnumerable<Assembly> GetAllAssembliesInProject() {
            return GameObject.FindObjectsOfType<MonoBehaviour>().Map(x => x.GetType().Assembly).Distinct();
        }

        private static void CheckTypesInAssembly(Assembly assembly) {
            foreach (var type in assembly.GetTypesWithMissingNamespace()) {
                Debug.LogError("(Assembly " + assembly.GetName().Name + ") Missing namespace: " + type);
            }
        }

    }

}
