using System.IO;
using System.Threading.Tasks;
using com.csutil.ui;
using UnityEngine;

namespace com.csutil {

    public static class GyroscopeExtensions {

        private static readonly Quaternion fix = Quaternion.Euler(90, 0, 0);

        /// <summary> Returns the device rotation in a format that cam be directly applied to the Unity camera.
        /// North will be in positive z Axis direction and East will be in positive x axis direction </summary>
        public static Quaternion attitudeV2ForUnityCamera(this Gyroscope self) {
            var q = self.attitude;
            //The Gyroscope is right-handed.  Unity is left handed. Make the necessary change to be used by the camera.
            // Idea from https://docs.unity3d.com/ScriptReference/Gyroscope.html 
            return fix * new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        public static void EnableIncludingCompass(this Gyroscope self, float desiredAccuracyInMeters = 10, float updateDistanceInMeters = 10) {
            Input.compass.enabled = true;
            Input.location.Start(desiredAccuracyInMeters, updateDistanceInMeters); // https://docs.unity3d.com/ScriptReference/LocationService.Start.html
            self.enabled = true;
        }

        public static async Task RequestFineLocationPermissionIfNeeded(this LocationService locationService,
            string message, string caption = "GPS positioning needs to be enabled") {
            {
                if (EnvironmentV2.isUnityEditor) { return; }
                while (!locationService.isEnabledByUser) {
                    await Dialog.ShowInfoDialog(
                        caption,
                        message,
                        "OK");
#if PLATFORM_ANDROID
                    if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)) {
                        UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
                        await TaskV2.Delay(1000);
                    }
#endif
                }
            }
        }

        public static int ToAngleInDegree(this ScreenOrientation self) {
            switch (self) {
                case ScreenOrientation.Portrait:
                    return 90;
                case ScreenOrientation.LandscapeLeft:
                    return 180;
                case ScreenOrientation.LandscapeRight:
                    return 0;
                case ScreenOrientation.PortraitUpsideDown:
                    return -90;
            }
            throw new InvalidDataException("Screen.orientation=" + self);
        }
        
    }

}