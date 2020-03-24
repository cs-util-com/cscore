using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Zio;

namespace com.csutil.io {

    class EnvironmentV2Unity : EnvironmentV2 {

        public bool isUnityEditor {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public EnvironmentV2Unity() : base(new UnitySystemInfo()) { }

        /// <summary>
        /// Base impl. would return C:\Users\User123\AppData\Local\Temp\ 
        /// Unity impl. will return C:\Users\User123\AppData\Local\Temp\DefaultCompany\UnityTestsA\
        /// </summary>
        protected override DirectoryInfo GetRootTempDirInfo() {
            return new DirectoryInfo(Application.temporaryCachePath);
        }

        public override DirectoryEntry GetRootAppDataFolder() {
            return new DirectoryInfo(Application.persistentDataPath).ToRootDirectoryEntry();
        }

        public override DirectoryEntry GetCurrentDirectory() {
            if (EnvironmentV2.isWebGL && !isUnityEditor) {
                AssertV2.IsTrue(Application.dataPath.StartsWith("http"), "Application.dataPath=" + Application.dataPath);
                return GetRootAppDataFolder();
            }
            return new DirectoryInfo(Application.dataPath).ToRootDirectoryEntry();
        }

    }

    internal class UnitySystemInfo : EnvironmentV2.ISystemInfo {
        // e.g. Arm, X32, Arm64, X64
        public string OSArchitecture { get; } = "" + RuntimeInformation.OSArchitecture;
        // On Win 10 => "Microsoft Windows 10.0.16299"
        // On macOS High Sierra 10.13.4 => "Darwin 17.5.0 Darwin Kernel Version 17.5.0 ..."
        public string OSDescription { get; } = RuntimeInformation.OSDescription;
        public string OSPlatForm { get; } = "" + Application.platform;
        // On Win 10 => "6.2.9200.0"
        // On macOS High Sierra 10.13.4 => "17.5.0.0"
        public string OSVersion { get; } = "" + Environment.OSVersion.Version;
        // e.g. Arm, X32, Arm64, X64
        public string ProcessArchitecture { get; } = "" + RuntimeInformation.ProcessArchitecture;
        // "Windows 7 (6.1.7601) 64bit" on 64 bit Windows 7
        // "Mac OS X 10.10.4" on Mac OS X Yosemite
        // "iPhone OS 8.4" on iOS 8.4
        // "Android OS API-22" on Android 5.1
        public string OperatingSystem { get; set; } = "" + SystemInfo.operatingSystem;
        // e.g. "iPhone6,1" or "PC"
        public string DeviceModel { get; set; } = "" + SystemInfo.deviceModel;
        // e.g. "Intel(R) Core(TM)2 Quad CPU Q6600 @ 2.40GHz"
        public string ProcessorType { get; set; } = "" + SystemInfo.processorType;
        public string AppId { get; set; } = "" + Application.identifier;
        public string AppName { get; set; } = "" + Application.productName;
        public string AppVersion { get; set; } = "" + Application.version;
        public string UnityVersion { get; set; } = "" + Application.unityVersion;
        public string culture { get; set; } = "" + CultureInfo.CurrentCulture;
        public string language { get; set; } = "" + Application.systemLanguage;
        public long runDateUtc { get; set; } = DateTime.Now.ToUnixTimestampUtc();
        public int utcOffset { get; set; } = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;
    }

}
