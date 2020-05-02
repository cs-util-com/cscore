using System.IO;
using UnityEngine;
using Zio;

namespace com.csutil.editor {

    public static class EditorIO {

        public static DirectoryEntry GetProjectFolder() { return new DirectoryInfo(Application.dataPath).Parent.ToRootDirectoryEntry(); }

        public static DirectoryEntry GetAssetsFolder() { return GetProjectFolder().GetChildDir("Assets"); }

    }

}
