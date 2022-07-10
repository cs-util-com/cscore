using System.Collections.Generic;
using System.IO;
using Zio;

namespace com.csutil.io {
    
    public class AppSecrets {

        public static Dictionary<string, string> Load() {
            return FindFile().LoadAs<Dictionary<string, string>>();
        }

        public static FileEntry FindFile(string fileName = "secrets-keys.txt") {
            var startDir = EnvironmentV2.instance.GetCurrentDirectory().GetFullFileSystemPath();
            return FindFile(fileName, new DirectoryInfo(startDir));
        }

        private static FileEntry FindFile(string fileName, DirectoryInfo dirToSearch) {
            if (!dirToSearch.IsNotNullAndExists()) {
                throw new FileNotFoundException($"Could not find secrets file '{fileName}'");
            }
            var file = dirToSearch.GetChild(fileName);
            if (file.IsNotNullAndExists()) { return file.ToFileEntryInNewRoot(); }
            return FindFile(fileName, dirToSearch.Parent);
        }

    }
    
}