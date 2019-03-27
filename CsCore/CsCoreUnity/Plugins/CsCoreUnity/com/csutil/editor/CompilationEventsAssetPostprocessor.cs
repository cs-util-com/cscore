using System;
using System.Media;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    class CompilationEventsAssetPostprocessor : AssetPostprocessor {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            Log(" !! AssetPostprocessor.OnPostprocessAllAssets");
        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod() { Log(" !! InitializeOnLoadMethod"); }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReloadScripts() {
            Log(" !! UnityEditor.Callbacks.DidReloadScripts");
            try { new SoundPlayer("C:/CompileSuccess.wav").Play(); } catch (Exception e) { Debug.LogWarning(e); }
        }

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoadMethod() { Log(" !! RuntimeInitializeOnLoadMethod"); }

        [UnityEditor.Callbacks.OnOpenAsset]
        static bool OnOpenAsset(int instanceID, int line) {
            Log(" !! UnityEditor.Callbacks.OnOpenAsset with instanceID=" + instanceID + ", line=" + line);
            return false; // see https://docs.unity3d.com/ScriptReference/Callbacks.OnOpenAssetAttribute.html
        }

        [UnityEditor.Callbacks.PostProcessBuild]
        static void PostProcessBuild(BuildTarget target, string pathToBuiltProject) {
            Log(" !! UnityEditor.Callbacks.PostProcessBuild with target=" + target + ", pathToBuiltProject=" + pathToBuiltProject);
        }

        [UnityEditor.Callbacks.PostProcessScene]
        public static void OnPostprocessScene() { Log(" !! UnityEditor.Callbacks.PostProcessScene"); }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Log(string message) { Debug.Log(message); }

    }

}
