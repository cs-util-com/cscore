using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

}
