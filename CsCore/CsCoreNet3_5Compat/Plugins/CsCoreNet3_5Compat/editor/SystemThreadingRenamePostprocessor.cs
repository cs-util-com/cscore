using UnityEngine;
using UnityEditor;
using System.IO;
using System;

/// <summary> This will ensure the System.Threading.dll is handled correctly based on the used .NET version </summary>
class SystemThreadingRenamePostprocessor : AssetPostprocessor {
    // About AssetPostprocessor see https://docs.unity3d.com/ScriptReference/AssetPostprocessor.OnPostprocessAllAssets.html

    private const string STDLL = "System.Threading.dll";
    private const string STDLL_BACKUP = STDLL + ".backup";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        Log(" !! AssetPostprocessor.OnPostprocessAllAssets");
        foreach (string str in importedAssets) {
            if (str.EndsWith(STDLL) || str.EndsWith(STDLL_BACKUP)) {
                RenameThreadingDllBasedOnDotNetVersion(new FileInfo(str));
            }
        }
    }

    private static void RenameThreadingDllBasedOnDotNetVersion(FileInfo threadingDll) {
        if (!threadingDll.Exists) { return; }
#if NET_4_6
        if (RenameFile(threadingDll, STDLL_BACKUP)) {
            Debug.LogError(STDLL + " not needed in .net 4.6+, deactivated dll " + threadingDll.FullName);
        }
#elif NET_2_0 || NET_2_0_SUBSET
        if (RenameFile(threadingDll, STDLL)) {
            Debug.LogError(STDLL + " needed previous to .net 4.6+, restored dll from " + threadingDll.FullName);
        }
#endif
    }

    private static bool RenameFile(FileInfo self, string newFileName) {
        var oldPath = self.FullName;
        var newPath = self.Directory.FullName + Path.DirectorySeparatorChar + newFileName;
        if (oldPath != newPath) { File.Move(oldPath, newPath); return true; }
        return false;
    }

    // Some more logging:

    [RuntimeInitializeOnLoadMethod]
    static void RuntimeInitializeOnLoadMethod() { Log(" !! RuntimeInitializeOnLoadMethod"); }

    [InitializeOnLoadMethod]
    static void InitializeOnLoadMethod() {
        Log(" !! InitializeOnLoadMethod"); 
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    static void DidReloadScripts() { Log(" !! UnityEditor.Callbacks.DidReloadScripts"); }

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
    public static void OnPostprocessScene() {
        Log(" !! UnityEditor.Callbacks.PostProcessScene");
    }

    [System.Diagnostics.Conditional("DEBUG")]
    private static void Log(string message) { Debug.Log(message); }

}