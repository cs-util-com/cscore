using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary> This will ensure the System.Threading.dll is handled correctly based on the used .NET version </summary>
class SystemThreadingRenamePostprocessor : AssetPostprocessor {
    // About AssetPostprocessor see https://docs.unity3d.com/ScriptReference/AssetPostprocessor.OnPostprocessAllAssets.html

    private const string STDLL = "System.Threading.dll";
    private const string STDLL_BACKUP = STDLL + ".backup";

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string str in importedAssets) {
            if (str.EndsWith(STDLL) || str.EndsWith(STDLL_BACKUP)) {
                renameThreadingDllIfNet4_6(new FileInfo(str));
            }
        }
    }

    private static void renameThreadingDllIfNet4_6(FileInfo threadingDll) {
        if (!threadingDll.Exists) { return; }
#if NET_4_6
        Debug.LogError(STDLL + " not needed in .net 4.6+, will rename it. " + threadingDll);
        RenameFile(threadingDll, STDLL_BACKUP);
#elif NET_2_0 || NET_2_0_SUBSET
        Debug.LogError(STDLL + " needed previous to .net 4.6+, will rename it. " + threadingDll);
        RenameFile(threadingDll, STDLL);
#endif
    }

    private static void RenameFile(FileInfo self, string newFileName) {
        var oldPath = self.FullName;
        var newPath = self.Directory.FullName + Path.DirectorySeparatorChar + newFileName;
        if (oldPath != newPath) { File.Move(oldPath, newPath); }
    }

}