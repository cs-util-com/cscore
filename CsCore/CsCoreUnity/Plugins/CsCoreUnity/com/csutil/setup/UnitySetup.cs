using com.csutil.http;
using com.csutil.io;
using com.csutil.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.csutil {

    public class UnitySetup {

        public static UnitySetup instance { get { return IoC.inject.GetOrAddSingleton<UnitySetup>(new object()); } }

        public virtual void Setup() {
            Log.instance = new LogViaUnityDebugLog();
            SystemConsoleToUnityLogRedirector.Setup();
            var initMainThread = MainThread.instance;
            IoC.inject.SetSingleton<RestFactory, UnityRestFactory>(new UnityRestFactory(), true);
            IoC.inject.SetSingleton<EnvironmentV2, EnvironmentV2Unity>(new EnvironmentV2Unity(), true);
        }
    }

}
