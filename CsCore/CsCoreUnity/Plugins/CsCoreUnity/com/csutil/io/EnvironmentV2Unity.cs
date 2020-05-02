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

        public EnvironmentV2Unity() : base(new UnitySystemInfo()) { }

        /// <summary>
        /// Base impl. would return C:\Users\User123\AppData\Local\Temp\ 
        /// Unity impl. will return C:\Users\User123\AppData\Local\Temp\DefaultCompany\UnityTestsA\
        /// </summary>
        protected override DirectoryInfo GetRootTempDirInfo() {
            return new DirectoryInfo(Application.temporaryCachePath);
        }

        public override DirectoryEntry GetOrAddAppDataFolder(string appDataSubfolderName) {
            return GetPersistentDataPath().GetChildDir(appDataSubfolderName).CreateV2().ToRootDirectoryEntry();
        }

        public override DirectoryEntry GetCurrentDirectory() {
            if (isWindows || isMacOs || isLinux || isUnityEditor) {
                var assetFolder = new DirectoryInfo(Application.dataPath);
                if (isUnityEditor) {
                    // Return "/Assets/TestApplicationData" to protect the rest of the Assets folder:
                    return assetFolder.GetChildDir("TestApplicationData").CreateV2().ToRootDirectoryEntry();
                }
                // On Windows, Linux and MacOS it makes sense to return the install folder:
                return assetFolder.ToRootDirectoryEntry();
            }
            // On all other platforms there is no install folder so return the normal GetPersistentDataPath:
            return GetPersistentDataPath().ToRootDirectoryEntry();
        }

        private static DirectoryInfo GetPersistentDataPath() { return new DirectoryInfo(Application.persistentDataPath); }

        public override CultureInfo CurrentCulture { get => Application.systemLanguage.ToCultureInfo(); set => throw new NotSupportedException(); }

        public override CultureInfo CurrentUICulture { get => CurrentCulture; set => CurrentCulture = value; }

        public static bool isUnityEditor {
            get {
#if UNITY_EDITOR
                return true;
#else
                return false;
#endif
            }
        }

        public static bool isWindows {
            get {
#if UNITY_STANDALONE_WIN 
                return true;
#else
                return false;
#endif
            }
        }

        public static bool isMacOs {
            get {
#if UNITY_STANDALONE_OSX 
                return true;
#else
                return false;
#endif
            }
        }

        public static bool isLinux {
            get {
#if UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
                return true;
#else
                return false;
#endif
            }
        }

        public static bool isAndroid {
            get {
#if UNITY_ANDROID
                return true;
#else
                return false;
#endif
            }
        }

        public static bool isIos {
            get {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

    }

    internal class UnitySystemInfo : EnvironmentV2.ISystemInfo {
        // e.g. Arm, X32, Arm64, X64
        public string oSArchitecture { get; set; } = "" + RuntimeInformation.OSArchitecture;
        // On Win 10 => "Microsoft Windows 10.0.16299"
        // On macOS High Sierra 10.13.4 => "Darwin 17.5.0 Darwin Kernel Version 17.5.0 ..."
        public string oSDescription { get; set; } = RuntimeInformation.OSDescription;
        public string osPlatform { get; set; } = "" + Application.platform;
        // On Win 10 => "6.2.9200.0"
        // On macOS High Sierra 10.13.4 => "17.5.0.0"
        public string oSVersion { get; set; } = "" + Environment.OSVersion.Version;
        // e.g. Arm, X32, Arm64, X64
        public string processArchitecture { get; set; } = "" + RuntimeInformation.ProcessArchitecture;
        // "Windows 7 (6.1.7601) 64bit" on 64 bit Windows 7
        // "Mac OS X 10.10.4" on Mac OS X Yosemite
        // "iPhone OS 8.4" on iOS 8.4
        // "Android OS API-22" on Android 5.1
        public string OperatingSystem { get; set; } = "" + SystemInfo.operatingSystem;
        // e.g. "iPhone6,1" or "PC"
        public string DeviceModel { get; set; } = "" + SystemInfo.deviceModel;
        // e.g. "Intel(R) Core(TM)2 Quad CPU Q6600 @ 2.40GHz"
        public string ProcessorType { get; set; } = "" + SystemInfo.processorType;
        public string appId { get; set; } = "" + Application.identifier;
        public string appName { get; set; } = "" + Application.productName;
        public string appVersion { get; set; } = "" + Application.version;
        public string UnityVersion { get; set; } = "" + Application.unityVersion;
        public string culture => "" + EnvironmentV2.instance.CurrentCulture;
        public string language { get; set; } = "" + Application.systemLanguage;
        public long latestLaunchDate { get; set; } = DateTimeV2.UtcNow.ToUnixTimestampUtc();
        public int utcOffset { get; set; } = TimeZoneInfo.Local.GetUtcOffset(DateTimeV2.UtcNow).Hours;
        public long? lastUpdateDate => IoC.inject.Get<IPreferences>(null)?.GetLastUpdateDate();
        public long? firstLaunchDate => IoC.inject.Get<IPreferences>(null)?.GetFirstStartDate();

    }

}
