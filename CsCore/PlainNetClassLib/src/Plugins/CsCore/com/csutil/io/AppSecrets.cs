using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.io {

    public interface AppSecrets {

        Task<string> GetSecret(string key);

    }

    /// <summary> An implementation of <see cref="AppSecrets"/> that can be used for local testing on a
    /// developer machine using test keys that are not used in production. The implementation looks for a
    /// key file in any parent folder of the folder the application is executed in, so that a
    /// developer can place such a key file e.g. in the root workspace folder. </summary>
    public class DevEnvSecretsForLocalTesting : AppSecrets {

        private const string DEFAULT_FILE_NAME = "Secrets for local dev testing.json.txt";

        private FileEntry file;

        public DevEnvSecretsForLocalTesting(string fileName = DEFAULT_FILE_NAME) {
            this.file = FindFile(fileName);
        }

        public Task<string> GetSecret(string key) {
            var keysDictionary = file.LoadAs<Dictionary<string, string>>();
            return Task.FromResult(keysDictionary[key]);
        }

        private static FileEntry FindFile(string fileName) {
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