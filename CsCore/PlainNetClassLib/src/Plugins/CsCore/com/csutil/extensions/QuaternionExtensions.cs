using System;
using System.Numerics;

namespace com.csutil {

    public static class QuaternionExtensions {

        public const double degreeToRad = Math.PI / 180d;
        public const double radToDegree = 180d / Math.PI;
        private const double halfPi = Math.PI / 2d;

        public static bool IsSimilarTo(this Quaternion q1, Quaternion q2, int digits) {
            return Math.Round(q1.X, digits) == Math.Round(q2.X, digits)
                && Math.Round(q1.Y, digits) == Math.Round(q2.Y, digits)
                && Math.Round(q1.Z, digits) == Math.Round(q2.Z, digits)
                && Math.Round(q1.W, digits) == Math.Round(q2.W, digits);
        }

        /// <summary> diff * q1 = q2  --->  diff = q2 * inverse(q1) </summary>
        public static Quaternion GetRotationDeltaTo(this Quaternion q1, Quaternion q2) { return Quaternion.Inverse(q1) * q2; }

        /// <summary> Returns pitch, yaw, roll in degrees </summary>
        public static Vector3 GetEulerAnglesAsPitchYawRoll(this Quaternion rotation) {
            if (Math.Abs(rotation.LengthSquared() - 1.0f) > 0.000001f) {
                throw new ArgumentException("Invalid quaternion (length!=1", nameof(rotation));
            }

            float sqx = rotation.X * rotation.X;
            float sqy = rotation.Y * rotation.Y;
            float sqz = rotation.Z * rotation.Z;
            float sqw = rotation.W * rotation.W;

            double sinPitch = 2d * (rotation.W * rotation.X - rotation.Z * rotation.Y);
            var sinPitchCloseTo1 = Math.Abs(sinPitch) > 0.999999;
            var pitch = sinPitchCloseTo1 ? halfPi * Math.Sign(sinPitch) : Math.Asin(sinPitch);

            var sinYaw = 2d * (rotation.W * rotation.Y + rotation.Z * rotation.X);
            var cosYaw = (sqw - sqx - sqy + sqz);
            if (sinPitchCloseTo1 && Math.Abs(sinYaw) < 0.000001f && Math.Abs(cosYaw) < 0.000001f) {
                return GetEulerAnglesAsPitchYawRoll(rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.00001f));
            }
            double yaw = Math.Atan2(sinYaw, cosYaw);

            double roll = Math.Atan2(2d * (rotation.W * rotation.Z + rotation.X * rotation.Y), sqw - sqx + sqy - sqz);

            if (double.IsNaN(pitch) || double.IsNaN(yaw) || double.IsNaN(roll)) {
                throw new Exception("Cant handle rot " + rotation);
            }
            return new Vector3((float)(pitch * radToDegree), (float)(yaw * radToDegree), (float)(roll * radToDegree));
        }

    }

}