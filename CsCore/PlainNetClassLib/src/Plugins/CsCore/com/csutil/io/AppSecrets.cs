using System.Collections.Generic;
using System.IO;
using Zio;

namespace com.csutil.io {

    public class AppSecrets {

        private const string DEFAULT_FILE_NAME = "Keys and secrets for local dev testing.txt";

        public static Dictionary<string, string> Load(string fileName = DEFAULT_FILE_NAME) {
            return FindFile(fileName).LoadAs<Dictionary<string, string>>();
        }

        public static FileEntry FindFile(string fileName = DEFAULT_FILE_NAME) {
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