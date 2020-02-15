using UnityEngine;

namespace com.csutil {

    public static class ApplicationV2 {

        public static bool IsAnyOf(this RuntimePlatform self, params RuntimePlatform[] platforms) {
            foreach (var p in platforms) { if (object.Equals(self, p)) { return true; } }
            return false;
        }

        public static RuntimePlatform platform {
            get {
#if UNITY_ANDROID
                 return RuntimePlatform.Android;
#elif UNITY_IOS
                 return RuntimePlatform.IPhonePlayer;
#elif UNITY_STANDALONE_OSX
                 return RuntimePlatform.OSXPlayer;
#elif UNITY_STANDALONE_WIN
                 return RuntimePlatform.WindowsPlayer;
#elif UNITY_WEBGL
                return RuntimePlatform.WebGLPlayer;
#else
#if UNITY_EDITOR
                Log.e("Running in editor so " + Application.platform + " will be returned instead of the correct target platform");
#endif
                return Application.platform;
#endif
            }
        }

        public static int targetFrameRateV2 {

#if UNITY_2019_3_OR_NEWER
            // See: 
            // - https://blogs.unity3d.com/2020/02/07/how-on-demand-rendering-can-improve-mobile-performance/
            // - https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Rendering.OnDemandRendering-effectiveRenderFrameRate.html
            set {
                UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = 1;
                if (UnityEngine.Rendering.OnDemandRendering.effectiveRenderFrameRate != value) {
                    if (QualitySettings.vSyncCount > 0) {
                        UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = (Screen.currentResolution.refreshRate / QualitySettings.vSyncCount / value);
                    } else {
                        UnityEngine.Rendering.OnDemandRendering.renderFrameInterval = (Application.targetFrameRate / value);
                    }
                }
            }
            get { return UnityEngine.Rendering.OnDemandRendering.effectiveRenderFrameRate; }
#else
            set { Application.targetFrameRate = value; }
            get { return Application.targetFrameRate; }
#endif
        }

    }

}
