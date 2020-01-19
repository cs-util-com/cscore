using System.IO;
using UnityEditor;

namespace com.csutil.editor {

    /// <summary> 
    /// This processor looks for empty folders in the Assets so that the developer can delete them,
    /// important when working with git since folders are not versioned but .meta files are and 
    /// each emtpy folder will get a .meta file created. 
    /// </summary>
    class EmptyFolderFinder : AssetPostprocessor {

        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReloadScripts() {
            var assets = EnvironmentV2.instance.GetCurrentDirectory();
            if (assets.NameV2() != "Assets") { throw Log.e("Root dir was not the Assets folder"); }
            LogAllEmptyFoldersIn(assets);
        }

        private static void LogAllEmptyFoldersIn(DirectoryInfo dir) {
            foreach (var d in dir.GetDirectories()) {
                if (d.IsEmtpy()) {
                    Log.e("Found an emtpy folder at: " + d);
                    d.OpenInExternalApp();
                } else {
                    LogAllEmptyFoldersIn(d);
                }
            }
        }
    }

}
