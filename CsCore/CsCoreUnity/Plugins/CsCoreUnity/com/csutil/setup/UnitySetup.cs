using com.csutil.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class UnitySetup {

        public static UnitySetup instance = new UnitySetup();

        public void setup() {
            Log.instance = new LogViaUnityDebugLog();
            SystemConsoleToUnityLogRedirector.Setup();
        }
    }

}
