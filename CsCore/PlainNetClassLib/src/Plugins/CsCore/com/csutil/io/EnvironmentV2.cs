using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Zio;

namespace com.csutil {

    public class EnvironmentV2 {

        public static EnvironmentV2 instance { get { return IoC.inject.GetOrAddSingleton<EnvironmentV2>(new object()); } }

        public readonly ISystemInfo systemInfo;
        private static char[] invalidChars;

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

        public virtual CultureInfo CurrentCulture {
            get => CultureInfo.CurrentCulture;
            set => CultureInfo.CurrentCulture = value;
        }

        public virtual CultureInfo CurrentUICulture {
            get => CultureInfo.CurrentUICulture != null ? CultureInfo.CurrentUICulture : CurrentCulture;
            set => CultureInfo.CurrentUICulture = value;
        }

        /// <summary> The folder of the binary (or dll that is executed) </summary>
        public virtual DirectoryEntry GetCurrentDirectory() {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).ToRootDirectoryEntry();
        }

        /// <summary> On Windows e.g. C:\Users\User123\AppData\Roaming\MyFolderName123 </summary>
        public virtual DirectoryEntry GetOrAddAppDataFolder(string appDataSubfolderName) {
            if (appDataSubfolderName.IsNullOrEmpty()) {
                throw new ArgumentNullException("Invalid appDataSubfolderName");
            }
            var appDataRoot = GetSpecialDirInfo(Environment.SpecialFolder.ApplicationData);
            return appDataRoot.GetChildDir(appDataSubfolderName).CreateV2().ToRootDirectoryEntry();
        }

        /// <summary> On Windows e.g. C:\Users\User123\AppData\Local\Temp\ </summary>
        public DirectoryEntry GetRootTempFolder() { return GetRootTempDirInfo().ToRootDirectoryEntry(); }

        protected virtual DirectoryInfo GetRootTempDirInfo() { return new DirectoryInfo(Path.GetTempPath()); }

        public DirectoryEntry GetOrAddTempFolder(string tempSubfolderName) {
            return GetRootTempFolder().GetChildDir(tempSubfolderName).CreateV2();
        }

        public DirectoryEntry GetSpecialFolder(Environment.SpecialFolder specialFolder) {
            return GetSpecialDirInfo(specialFolder).ToRootDirectoryEntry();
        }

        protected virtual DirectoryInfo GetSpecialDirInfo(Environment.SpecialFolder specialFolder) {
            return new DirectoryInfo(Environment.GetFolderPath(specialFolder));
        }

        public virtual DirectoryEntry GetNewInMemorySystem() {
            return new DirectoryEntry(new Zio.FileSystems.MemoryFileSystem(), UPath.Root);
        }

        public static string SanatizeToFileName(string fileName) {
            if (invalidChars == null) { invalidChars = Path.GetInvalidFileNameChars().AddViaUnion(':').ToArray(); }
            return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
        }

        public interface ISystemInfo {
            string oSArchitecture { get; }
            string oSDescription { get; }
            string osPlatform { get; }
            string oSVersion { get; }
            string processArchitecture { get; }
            string appId { get; }
            string appName { get; }
            string appVersion { get; }
            string culture { get; }
            string language { get; }
            long? lastUpdateDate { get; }
            long? firstLaunchDate { get; }
            long latestLaunchDate { get; }
            int utcOffset { get; }
        }

        public class SystemInfo : ISystemInfo {
            // e.g. Arm, X32, Arm64, X64
            public string oSArchitecture { get; set; } = "" + RuntimeInformation.OSArchitecture;
            // On Win 10 => "Microsoft Windows 10.0.16299"
            // On macOS High Sierra 10.13.4 => "Darwin 17.5.0 Darwin Kernel Version 17.5.0 ..."
            public string oSDescription { get; set; } = RuntimeInformation.OSDescription;
            // On Win 10 => "Win32NT"
            // On macOS High Sierra 10.13.4 => "Unix"
            public string osPlatform { get; set; } = "" + Environment.OSVersion.Platform;
            // On Win 10 => "6.2.9200.0"
            // On macOS High Sierra 10.13.4 => "17.5.0.0"
            public string oSVersion { get; set; } = "" + Environment.OSVersion.Version;
            // e.g. Arm, X32, Arm64, X64
            public string processArchitecture { get; set; } = "" + RuntimeInformation.ProcessArchitecture;
            public string appId { get; set; } = "" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            public string appName { get; set; } = "" + AppDomain.CurrentDomain.FriendlyName;
            public string appVersion { get; set; } = "" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            public string culture { get; set; } = "" + CultureInfo.CurrentCulture;
            public string language { get; set; } = "" + CultureInfo.CurrentCulture.EnglishName;
            public long latestLaunchDate { get; set; } = DateTimeV2.UtcNow.ToUnixTimestampUtc();
            public int utcOffset { get; set; } = TimeZoneInfo.Local.GetUtcOffset(DateTimeV2.UtcNow).Hours;
            public long? lastUpdateDate => IoC.inject.Get<IPreferences>(null)?.GetLastUpdateDate();
            public long? firstLaunchDate => IoC.inject.Get<IPreferences>(null)?.GetFirstStartDate();

        }

    }

}