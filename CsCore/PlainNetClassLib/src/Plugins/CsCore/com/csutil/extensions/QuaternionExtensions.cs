using System;
using System.Numerics;

namespace com.csutil {

    public static class QuaternionExtensions {

        public static bool IsSimilarTo(this Quaternion q1, Quaternion q2, int digits) {
            return Math.Round(q1.X, digits) == Math.Round(q2.X, digits)
                && Math.Round(q1.Y, digits) == Math.Round(q2.Y, digits)
                && Math.Round(q1.Z, digits) == Math.Round(q2.Z, digits)
                && Math.Round(q1.W, digits) == Math.Round(q2.W, digits);
        }

    }

}