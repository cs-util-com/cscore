using com.csutil.http;
using com.csutil.io;
using com.csutil.logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace com.csutil {

    public class UnitySetup {

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup() {
            Log.instance = new LogViaUnityDebugLog();
            SystemConsoleToUnityLogRedirector.Setup();
            var initMainThread = MainThread.instance;
            IoC.inject.SetSingleton<EnvironmentV2, EnvironmentV2Unity>(new EnvironmentV2Unity(), true);
            IoC.inject.SetSingleton<RestFactory, UnityRestFactory>(new UnityRestFactory(), true);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoaded() {
            Log.d("Now the scene finished loading");
        }

    }

}
