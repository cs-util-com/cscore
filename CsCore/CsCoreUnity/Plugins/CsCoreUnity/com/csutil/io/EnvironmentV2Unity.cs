using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil.io {

    class EnvironmentV2Unity : EnvironmentV2 {

        public override DirectoryInfo GetTempFolder() {
            var d = new DirectoryInfo(Application.temporaryCachePath);
            AssertV2.AreEqual(base.GetTempFolder().FullPath(), d.FullPath()); // TODO test this
            return d;
        }

    }

}
