using System;
using System.Numerics;

namespace com.csutil {

    public static class QuaternionExtensions {

        public const double degreeToRad = Math.PI / 180d;
        public const double radToDegree = 180d / Math.PI;
        private const double halfPi = Math.PI / 2d;

        public static bool Equals(this Quaternion q1, Quaternion q2, int digits) {
            return Math.Round(q1.X, digits) == Math.Round(q2.X, digits)
                && Math.Round(q1.Y, digits) == Math.Round(q2.Y, digits)
                && Math.Round(q1.Z, digits) == Math.Round(q2.Z, digits)
                && Math.Round(q1.W, digits) == Math.Round(q2.W, digits);
        }

        public static bool IsSimilarTo(this Quaternion q1, Quaternion q2, double degreeDifferenceThreshold = 0.1) {
            return q1.GetRotationDeltaInDegreeTo(q2) < degreeDifferenceThreshold;
        }

        public static double GetRotationDeltaInDegreeTo(this Quaternion q1, Quaternion q2) {
            return GetRotationDeltaInRadTo(q1, q2) * radToDegree;
        }

        public static double GetRotationDeltaInRadTo(this Quaternion q1, Quaternion q2) {
            return Math.Acos(Math.Min(Math.Abs(Quaternion.Dot(q1, q2)), 1.0)) * 2.0;
        }

        public static Matrix4x4 ToMatrix4X4(this Quaternion self) { return Matrix4x4.CreateFromQuaternion(self); }
        
        /// <summary> diff * q1 = q2  --->  diff = q2 * inverse(q1) </summary>
        public static Quaternion GetRotationDeltaTo(this Quaternion q1, Quaternion q2) { return Quaternion.Inverse(q1) * q2; }

        /// <summary>
        /// Returns pitch, yaw, roll in degrees
        /// - Yaw is the rotation around the up axis (compass heading)
        /// - Roll is the rotation around the forward axis
        /// - Pitch is 0 when looking at the horizon and 90 when looking straight down
        /// </summary>
        /// <param name="returnDegree"> false to return the angles in radians </param>
        public static Vector3 GetEulerAnglesAsPitchYawRoll(this Quaternion rotation, bool returnDegree = true) {
            var length = rotation.LengthSquared();
            if (Math.Abs(length - 1.0f) > 0.0001f) {
                throw new ArgumentException($"Invalid rotation quaternion (length not 1 but {length})", nameof(rotation));
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
                return GetEulerAnglesAsPitchYawRoll(rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.00001f));
            }
            double yaw = Math.Atan2(sinYaw, cosYaw);
            if (yaw <= -Math.PI + 0.000001) { yaw += Math.PI * 2d; } // Visual fix for yaw being -180° (no effect on math correctness)

            double roll = Math.Atan2(2d * (rotation.W * rotation.Z + rotation.X * rotation.Y), sqw - sqx + sqy - sqz);

            if (returnDegree) {
                return new Vector3((float)(pitch * radToDegree), (float)(yaw * radToDegree), (float)(roll * radToDegree));
            } else {
                return new Vector3((float)pitch, (float)yaw, (float)roll);
            }
        }

        /// <summary> Rotates a vector by the given quaternion </summary>
        /// <param name="roundResult"> by default is set to true to compensate for small errors in the quaternion math </param>
        public static Vector3 Rotate(this Quaternion self, Vector3 vector, bool roundResult = true) {
            var rotated = Vector3.Transform(vector, self);
            if (roundResult) { rotated = rotated.Round(6); }
            return rotated;
        }

        public static Quaternion LookRotation(Vector3 directionVector, Vector3 upVector) {
            var dir = Vector3.Normalize(directionVector);
            var right = Vector3.Normalize(Vector3.Cross(upVector, dir));
            var up = Vector3.Cross(dir, right);
            var rotMat = new Matrix4x4(
                right.X, right.Y, right.Z, 0,
                up.X, up.Y, up.Z, 0,
                dir.X, dir.Y, dir.Z,
                0, 0, 0, 0, 1);
            return Quaternion.CreateFromRotationMatrix(rotMat);
        }

    }

}