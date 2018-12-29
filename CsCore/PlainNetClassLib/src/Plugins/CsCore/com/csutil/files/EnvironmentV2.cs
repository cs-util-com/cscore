using System;
using System.IO;

namespace com.csutil {
    public class EnvironmentV2 {

        public static EnvironmentV2 instance { get { return IoC.inject.GetOrAddSingleton<EnvironmentV2>(new object()); } }

        public virtual DirectoryInfo GetCurrentDirectory() {
            return new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        public DirectoryInfo GetAppDataFolder() {
            return GetSpecialFolder(Environment.SpecialFolder.ApplicationData);
        }

        public virtual DirectoryInfo GetSpecialFolder(Environment.SpecialFolder specialFolder) {
            return new DirectoryInfo(Environment.GetFolderPath(specialFolder));
        }

    }
}