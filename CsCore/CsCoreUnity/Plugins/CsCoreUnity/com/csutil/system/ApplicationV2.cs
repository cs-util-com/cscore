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
            set {
                // Start off assuming that Application.targetFrameRate is 60 and QualitySettings.vSyncCount is 0
                OnDemandRendering.renderFrameInterval = 6;

                // Some applications may allow the user to modify the quality level. So we may not be able to rely on
                // the framerate always being a specific value. For this example we want the effective framerate to be value.
                // If it is not then check the values and adjust the frame interval accordingly to achieve the framerate that we desire.
                if (OnDemandRendering.effectiveRenderFrameRate != value) {
                    if (QualitySettings.vSyncCount > 0) {
                        OnDemandRendering.renderFrameInterval = (Screen.currentResolution.refreshRate / QualitySettings.vSyncCount / value);
                    } else {
                        OnDemandRendering.renderFrameInterval = (Application.targetFrameRate / value);
                    }
                }
            }
            get { return OnDemandRendering.effectiveRenderFrameRate; }
#else
            set { Application.targetFrameRate = value; }
            get { return Application.targetFrameRate; }
#endif
        }

    }

}
