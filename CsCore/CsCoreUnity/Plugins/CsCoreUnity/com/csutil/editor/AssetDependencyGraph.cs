using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.csutil.editor {

    /// <summary> Forked the idea from https://github.com/Facepunch/WhatUsesThis </summary>
    static class AssetDependencyGraph {

        public const int menuEntryPosInAssetsMenu = 25;

        private static Dictionary<string, List<string>> _dict;
        private static Dictionary<string, List<string>> Dict => _dict ?? LoadDependencyGraphFromFile() ?? RebuildDependencyGraph();

        [MenuItem("Window/CsUtil/CsCore/Force Rebuild Of Asset Dependency Graph")]
        static Dictionary<string, List<string>> RebuildDependencyGraph() {
            try {
                var t = Log.MethodEntered();

                var progressBarTitle = "Asset Dependency Graph";
                EditorUtility.DisplayProgressBar(progressBarTitle, "Resolving Assets", 0.2f);

                var allAssets = AssetDatabase.FindAssets("").Select(x => AssetDatabase.GUIDToAssetPath(x)).Distinct().ToArray();

                var dependencies = new Dictionary<string, string[]>();

                var assetCounter = 0;
                foreach (var asset in allAssets) {
                    dependencies[asset] = AssetDatabase.GetDependencies(asset, false);
                    assetCounter++;

                    if (assetCounter % 100 == 0) { // Every 100 assets show progress update:
                        var progressText = $"Resolving Dependencies [{assetCounter}/{allAssets.Length}]";
                        var percentProcessed = assetCounter / (float)allAssets.Length;
                        if (EditorUtility.DisplayCancelableProgressBar(progressBarTitle, progressText, percentProcessed)) {
                            return new Dictionary<string, List<string>>(); // User canceled process
                        }
                    }
                }

                EditorUtility.DisplayProgressBar(progressBarTitle, "Building Graph", 0.9f);

                _dict = new Dictionary<string, List<string>>();
                foreach (var dep in dependencies) {
                    foreach (var dependant in dep.Value) {
                        if (!_dict.TryGetValue(dependant, out var list)) {
                            list = new List<string>();
                            _dict[dependant] = list;
                        }
                        list.Add(dep.Key);
                    }
                }

                SaveDependencyGraphToFile();
                Log.MethodDone(t);
                return _dict;
            }
            finally { EditorUtility.ClearProgressBar(); }
        }

        static void SaveDependencyGraphToFile() {
            if (_dict == null) { return; }
            var t = Log.MethodEntered();
            using (var stream = GetCacheFile().OpenOrCreateForWrite()) {
                new BinaryFormatter().Serialize(stream, _dict);
            }
            Log.MethodDone(t);
        }

        private static Zio.FileEntry GetCacheFile() {
            return EnvironmentV2.instance.GetCurrentDirectory().GetChild("CachedAssetDependencyGraph.bin");
        }

        static Dictionary<string, List<string>> LoadDependencyGraphFromFile() {
            var t = Log.MethodEntered();
            try {
                using (var stream = GetCacheFile().OpenForRead()) {
                    _dict = (Dictionary<string, List<string>>)new BinaryFormatter().Deserialize(stream);
                }
            }
            catch (System.Exception) { _dict = null; }
            Log.MethodDone(t);
            return _dict;
        }

        [MenuItem("Assets/Log References In Project (Used by Assets)", false, menuEntryPosInAssetsMenu + 1)]
        private static void LogUsedByRefsInProject() {
            int count = 0;
            foreach (var selectedObj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
                var selected = AssetDatabase.GetAssetPath(selectedObj);
                Log.d($"<b>{selected}</b> is used by:", selectedObj);
                if (Dict.TryGetValue(selected, out var dependants)) {
                    foreach (var d in dependants) {
                        Log.d($"<color=#7BA039>        - {d}</color>", AssetDatabase.LoadAssetAtPath<Object>(d));
                        count++;
                    }
                }
            }
            Log.d($"<b>Search complete, found {count} result(s)</b>");
        }

        [MenuItem("Assets/Log Dependencies In Project (Uses Assets)", false, menuEntryPosInAssetsMenu + 2)]
        private static void LogUsesDependenciesInProject() {
            int count = 0;
            foreach (var selectedObj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets)) {
                var selected = AssetDatabase.GetAssetPath(selectedObj);
                Log.d($"<b>{selected}</b> uses:", selectedObj);
                foreach (var d in AssetDatabase.GetDependencies(selected, false)) {
                    Log.d($"<color=#7BA039>        - {d}</color>", AssetDatabase.LoadAssetAtPath<Object>(d));
                    count++;
                }
            }
            Log.d($"<b>Search complete, found {count} result(s)</b>");
        }

    }

}