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

    }

}
