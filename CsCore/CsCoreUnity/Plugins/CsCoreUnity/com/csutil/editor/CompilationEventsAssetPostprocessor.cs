using System;
using System.Media;
using UnityEditor;
using UnityEngine;

namespace com.csutil.editor {

    class CompilationEventsAssetPostprocessor : AssetPostprocessor {

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            Logd(" !! AssetPostprocessor.OnPostprocessAllAssets");
        }

        [InitializeOnLoadMethod]
        static void InitializeOnLoadMethod() { Logd(" !! InitializeOnLoadMethod"); }

        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReloadScripts() {
            UnitySetup.SetupDefaultSingletonsIfNeeded();
            Logd(" !! UnityEditor.Callbacks.DidReloadScripts");
            try { new SoundPlayer("C:/CompileSuccess.wav").Play(); } catch (Exception e) { Debug.LogWarning(e); }
        }

        [RuntimeInitializeOnLoadMethod]
        static void RuntimeInitializeOnLoadMethod() { Logd(" !! RuntimeInitializeOnLoadMethod"); }

        [UnityEditor.Callbacks.OnOpenAsset]
        static bool OnOpenAsset(int instanceID, int line) {
            Logd(" !! UnityEditor.Callbacks.OnOpenAsset with instanceID=" + instanceID + ", line=" + line);
            return false; // see https://docs.unity3d.com/ScriptReference/Callbacks.OnOpenAssetAttribute.html
        }

        [UnityEditor.Callbacks.PostProcessBuild]
        static void PostProcessBuild(BuildTarget target, string pathToBuiltProject) {
            Logd(" !! UnityEditor.Callbacks.PostProcessBuild with target=" + target + ", pathToBuiltProject=" + pathToBuiltProject);
        }

        [UnityEditor.Callbacks.PostProcessScene]
        public static void OnPostprocessScene() { Logd(" !! UnityEditor.Callbacks.PostProcessScene"); }

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Logd(string message) { Debug.Log(message); }

    }

}
