using com.csutil.http;
using com.csutil.io;
using com.csutil.logging;
using System;
using UnityEngine;

namespace com.csutil {

    public class UnitySetup {

        public const string UNITY_SETUP_DONE = "Unity setup now done";
         
        static UnitySetup() { // This method is only executed only once at the very beginning 
            Debug.Log("com.csutil.UnitySetup initializing..");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Setup() {
            Log.instance = new LogToUnityDebugLog();
            SystemConsoleToUnityLogRedirector.Setup();
            var initMainThread = MainThread.instance; // Called to init main thread
            IoC.inject.SetSingleton<EnvironmentV2, EnvironmentV2Unity>(new EnvironmentV2Unity(), true);
            IoC.inject.SetSingleton<RestFactory, UnityRestFactory>(new UnityRestFactory(), true);
            EventBus.instance.Publish(UNITY_SETUP_DONE);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void OnAfterSceneLoaded() { Log.d("Now the scene finished loading"); }

        /// <summary> 
        /// Ensures that the callback is invoked either directly if the UnitySetup already ran or 
        /// after the UnitySetup is fully initialized 
        /// </summary>
        public static void InvokeAfterUnitySetupDone(Action callback) {
            EventBus.instance.SubscribeForOnePublishOrInstantInvokeIfInHistory(UNITY_SETUP_DONE, callback);
        }

    }

}
