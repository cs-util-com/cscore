using System;
using System.IO;
using UnityEngine;
using Zio;

namespace com.csutil {

    public static class ApplicationV2 {

        public static bool IsAnyOf(this RuntimePlatform self, params RuntimePlatform[] platforms) {
            foreach (var p in platforms) {
                if (object.Equals(self, p)) { return true; }
            }
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

        private static bool _isPlaying = true;
        public static bool isPlaying {
            get {
                try { _isPlaying = Application.isPlaying; } catch (Exception) { }
                return _isPlaying;
            }
        }

        public static DirectoryEntry dataPath {
            get { return new DirectoryInfo(Application.dataPath).Parent.ToRootDirectoryEntry().GetChildDir("Assets"); }
        }

        public static bool IsEditorOnValidateAllowed() {
            if (isPlaying) { return false; }
            var s = GameObject.Find(InjectorExtensionsForUnity.DEFAULT_SINGLETON_NAME);
            /* There seems to be a strange Unity editor bug that can cause Unity to crash
             * if a root transform in the scene is interacted with while the scene is still 
             * initializing (e.g during Editor startup). 
             */
            var unityCouldCrash = s == null || s.transform.GetSiblingIndex() == 0;
            // if (unityCouldCrash) { Log.w("Unity currently initializing, abording to avoid Unity crash"); }
            return !unityCouldCrash;
        }

        public static void Quit() {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #elif UNITY_WEBPLAYER
            Application.OpenURL("about:blank");
            #else
            Application.Quit();
            #endif
        }
        
    }

}