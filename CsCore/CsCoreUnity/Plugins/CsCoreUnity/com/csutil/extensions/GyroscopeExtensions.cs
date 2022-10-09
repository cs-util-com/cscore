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

    }

}