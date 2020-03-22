using System;
using System.IO;
using System.Runtime.InteropServices;
using Zio;

namespace com.csutil {

    public class EnvironmentV2 {

        public static EnvironmentV2 instance { get { return IoC.inject.GetOrAddSingleton<EnvironmentV2>(new object()); } }

        public readonly ISystemInfo systemInfo;

        public EnvironmentV2() : this(new SystemInfo()) { }
        protected EnvironmentV2(ISystemInfo systemInfo) { this.systemInfo = systemInfo; }

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
        public DirectoryEntry GetRootTempFolder() { return GetRootTempDirInfo().ToRootDirectoryEntry(); }

        protected virtual DirectoryInfo GetRootTempDirInfo() { return new DirectoryInfo(Path.GetTempPath()); }

        public virtual DirectoryEntry GetOrAddTempFolder(string tempSubfolderName) {
            return GetRootTempDirInfo().GetChildDir(tempSubfolderName).CreateV2().ToRootDirectoryEntry();
        }

        public virtual DirectoryEntry GetSpecialFolder(Environment.SpecialFolder specialFolder) {
            return new DirectoryInfo(Environment.GetFolderPath(specialFolder)).ToRootDirectoryEntry();
        }

        public interface ISystemInfo {
            string OSArchitecture { get; }
            string OSDescription { get; }
            string OSPlatForm { get; }
            string OSVersion { get; }
            string ProcessArchitecture { get; }
        }

        public class SystemInfo : ISystemInfo {
            // e.g. Arm, X32, Arm64, X64
            public string OSArchitecture { get; } = "" + RuntimeInformation.OSArchitecture;
            // On Win 10 => "Microsoft Windows 10.0.16299"
            // On macOS High Sierra 10.13.4 => "Darwin 17.5.0 Darwin Kernel Version 17.5.0 ..."
            public string OSDescription { get; } = RuntimeInformation.OSDescription;
            // On Win 10 => "Win32NT"
            // On macOS High Sierra 10.13.4 => "Unix"
            public string OSPlatForm { get; } = "" + Environment.OSVersion.Platform;
            // On Win 10 => "6.2.9200.0"
            // On macOS High Sierra 10.13.4 => "17.5.0.0"
            public string OSVersion { get; } = "" + Environment.OSVersion.Version;
            // e.g. Arm, X32, Arm64, X64
            public string ProcessArchitecture { get; } = "" + RuntimeInformation.ProcessArchitecture;
        }

    }

}