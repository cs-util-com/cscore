using System;
using System.IO;

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
        public virtual DirectoryInfo GetCurrentDirectory() {
            return new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        public virtual DirectoryInfo GetRootAppDataFolder() {
            return GetSpecialFolder(Environment.SpecialFolder.ApplicationData);
        }

        /// <summary> On Windows e.g. C:\Users\User123\AppData\Local\Temp\ </summary>
        public virtual DirectoryInfo GetRootTempFolder() {
            return new DirectoryInfo(Path.GetTempPath());
        }

        public virtual DirectoryInfo GetOrAddTempFolder(string tempSubfolderName) {
            return GetRootTempFolder().GetChildDir(tempSubfolderName).CreateV2();
        }

        public virtual DirectoryInfo GetSpecialFolder(Environment.SpecialFolder specialFolder) {
            return new DirectoryInfo(Environment.GetFolderPath(specialFolder));
        }

    }

}