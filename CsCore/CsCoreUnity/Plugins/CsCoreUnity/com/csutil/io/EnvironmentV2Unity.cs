using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.io {

    class EnvironmentV2Unity : EnvironmentV2 {

        /// <summary>
        /// Base impl. would return C:\Users\User123\AppData\Local\Temp\ 
        /// Unity impl. will return C:\Users\User123\AppData\Local\Temp\DefaultCompany\UnityTestsA\
        /// </summary>
        public override DirectoryInfo GetRootTempFolder() {
            return new DirectoryInfo(Application.temporaryCachePath);
        }

    }

}
