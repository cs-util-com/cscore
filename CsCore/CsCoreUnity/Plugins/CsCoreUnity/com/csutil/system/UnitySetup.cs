using com.csutil.async;
using com.csutil.http;
using com.csutil.injection;
using com.csutil.io;
using com.csutil.logging;
using System;
using UnityEngine;

namespace com.csutil {

    public static class UnitySetup {

        public const string UNITY_SETUP_DONE = "Unity setup now done";

        static UnitySetup() { // This method is only executed only once after recompile 
            // Debug.Log("com.csutil.UnitySetup static constructor called..");
        }

#if UNITY_2019_3_OR_NEWER // <- Check needed? TODO
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif  // See https://blogs.unity3d.com/2019/11/05/enter-play-mode-faster-in-unity-2019-3/
        public static void ResetAllStaticObjects() {
            // Debug.Log("ResetAllStaticObjects");
            EventBus.instance = new EventBus();
            IoC.inject = Injector.newInjector(EventBus.instance);
            Log.instance = new LogToUnityDebugLog();
            var caller = new object();
            IoC.inject.SetSingleton<EnvironmentV2>(new EnvironmentV2Unity(), true);
            if (EnvironmentV2.isWebGL) { IoC.inject.SetSingleton<TaskV2>(new TaskV2WebGL(), true); }

            { // Setup an UnityRestFactoryV2 only if there is not already a RestFactory injected
                var restFactory = IoC.inject.GetOrAddSingleton<IRestFactory>(null, () => new UnityRestFactoryV2());
                if (!(restFactory is UnityRestFactoryV2)) {
                    Log.d($"Will NOT use {nameof(UnityRestFactoryV2)} since a {restFactory.GetType().Name} was already present");
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad() {
            // Debug.Log("BeforeSceneLoad");
            SystemConsoleToUnityLogRedirector.Setup();
            try { DestroyExistingMainThreadIfNeeded(); } catch (Exception e) { Debug.Log(e); }
            MainThread.instance.enabled = true; // Called to init main thread if not yet done by other logic
        }

        private static void DestroyExistingMainThreadIfNeeded() {
            var mt = IoC.inject.Get<MainThread>(null, false);
            if (mt != null) { mt.gameObject.Destroy(); }
        }

        /// <summary> This will be called after all components in the scene already triggered their initialization logic </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad() { EventBus.instance.Publish(UNITY_SETUP_DONE); }

        /// <summary> 
        /// Ensures that the callback is invoked either directly if the UnitySetup already ran or 
        /// after the UnitySetup is fully initialized 
        /// </summary>
        public static void InvokeAfterUnitySetupDone(object caller, Action callback) {
            EventBus.instance.SubscribeForOnePublish(caller, UNITY_SETUP_DONE, callback);
        }

    }

}
