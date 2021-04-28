using System.IO;
using UnityEditor;
using UnityEngine;
using Zio;

namespace com.csutil.editor {

    public static class EditorIO {

        public static DirectoryEntry GetProjectFolder() {
            return new DirectoryInfo(Application.dataPath).Parent.ToRootDirectoryEntry();
        }

        public static DirectoryEntry GetAssetsFolder() {
            return GetProjectFolder().GetChildDir("Assets");
        }

        public static DirectoryEntry OpenFolderPanelV2(string title) {
            var selectedDir = new DirectoryInfo(EditorUtility.OpenFolderPanel(title, "", ""));
            return selectedDir.Parent.ToRootDirectoryEntry().GetChildDir(selectedDir.Name);
        }

        public static DirectoryEntry GetFolderOfCurrentSelectedObject() {
            return new DirectoryInfo(AssetDatabase.GetAssetPath(Selection.activeObject)).ToRootDirectoryEntry();
        }

    }

}
