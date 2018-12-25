using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace com.csutil {

    class IO {

        public static DirectoryInfo cachingFolder { get { return new DirectoryInfo(Application.temporaryCachePath); } }

    }

}
