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

        public const string UNITY_SETUP_DONE = "Unity setup now done";

        /// <summary> 
        /// Ensures that the callback is invoked either directly if the UnitySetup already ran or 
        /// after the UnitySetup is fully initialized 
        /// </summary>
        public static void InvokeAfterUnitySetupDone(Action callback) {
            EventBus.instance.SubscribeForOnePublishOrInstantInvokeIfInHistory(UNITY_SETUP_DONE, callback);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup() {
            Log.instance = new LogToUnityDebugLog();
            SystemConsoleToUnityLogRedirector.Setup();
            var initMainThread = MainThread.instance;
            IoC.inject.SetSingleton<EnvironmentV2, EnvironmentV2Unity>(new EnvironmentV2Unity(), true);
            IoC.inject.SetSingleton<RestFactory, UnityRestFactory>(new UnityRestFactory(), true);
            EventBus.instance.Publish(UNITY_SETUP_DONE);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoaded() {
            Log.d("Now the scene finished loading");
        }

    }

}
