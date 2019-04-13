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
            SystemConsoleToUnityLogRedirector.Setup();
            SetupDefaultSingletonsIfNeeded();
            EventBus.instance.Publish(UNITY_SETUP_DONE);
        }

        public static void SetupDefaultSingletonsIfNeeded() {
            var initMainThread = MainThread.instance; // Called to init main thread
            var caller = new object();
            Log.instance = IoC.inject.GetOrAddSingleton<ILog>(caller, () => new LogToUnityDebugLog());
            IoC.inject.GetOrAddSingleton<EnvironmentV2>(caller, () => new EnvironmentV2Unity());
            IoC.inject.GetOrAddSingleton<RestFactory>(caller, () => new UnityRestFactory());
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
