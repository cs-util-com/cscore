using UnityEditor;
using Zio;

namespace com.csutil.editor {

    /// <summary> 
    /// This processor looks for empty folders in the Assets so that the developer can delete them,
    /// important when working with git since folders are not versioned but .meta files are and 
    /// each emtpy folder will get a .meta file created. 
    /// </summary>
    class EmptyFolderFinder : AssetPostprocessor {

        [UnityEditor.Callbacks.DidReloadScripts]
        static void DidReloadScripts() {
            LogAllEmptyFoldersIn(EditorIO.GetAssetsFolder());
        }

        private static void LogAllEmptyFoldersIn(DirectoryEntry assetsFolder) {
            foreach (var d in assetsFolder.GetDirectories()) {
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
