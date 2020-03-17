using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Zio;
using Zio.FileSystems;

namespace com.csutil.io {

    class EnvironmentV2Unity : EnvironmentV2 {

        /// <summary>
        /// Base impl. would return C:\Users\User123\AppData\Local\Temp\ 
        /// Unity impl. will return C:\Users\User123\AppData\Local\Temp\DefaultCompany\UnityTestsA\
        /// </summary>
        public override DirectoryEntry GetRootTempFolder() {
            return new DirectoryInfo(Application.temporaryCachePath).ToRootDirectoryEntry();
        }

        public override DirectoryEntry GetRootAppDataFolder() {
            return new DirectoryInfo(Application.persistentDataPath).ToRootDirectoryEntry();
        }

        public override DirectoryEntry GetCurrentDirectory() {
            return new DirectoryInfo(Application.dataPath).ToRootDirectoryEntry();
        }

    }

}
