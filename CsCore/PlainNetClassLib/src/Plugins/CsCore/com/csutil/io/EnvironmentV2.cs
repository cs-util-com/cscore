using System;
using System.IO;
using Zio;

namespace com.csutil {

    public class EnvironmentV2 {

        public static EnvironmentV2 instance { get { return IoC.inject.GetOrAddSingleton<EnvironmentV2>(new object()); } }

        public static bool isWebGL {
            get {
#if UNITY_WEBGL
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary> The folder of the binary (or dll that is executed) </summary>
        public virtual DirectoryEntry GetCurrentDirectory() {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).ToRootDirectoryEntry();
        }

        public virtual DirectoryEntry GetRootAppDataFolder() {
            return GetSpecialFolder(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary> On Windows e.g. C:\Users\User123\AppData\Local\Temp\ </summary>
        public virtual DirectoryEntry GetRootTempFolder() {
            return new DirectoryInfo(Path.GetTempPath()).ToRootDirectoryEntry();
        }

        public virtual DirectoryEntry GetOrAddTempFolder(string tempSubfolderName) {
            return GetRootTempFolder().GetChildDir(tempSubfolderName).CreateV2();
        }

        public virtual DirectoryEntry GetSpecialFolder(Environment.SpecialFolder specialFolder) {
            return new DirectoryInfo(Environment.GetFolderPath(specialFolder)).ToRootDirectoryEntry();
        }

    }

}