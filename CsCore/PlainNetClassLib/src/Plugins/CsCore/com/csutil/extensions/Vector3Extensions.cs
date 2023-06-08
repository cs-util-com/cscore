﻿using System;
using System.Numerics;

namespace com.csutil {

    public static class Vector3Extensions {

        public const double DEGREE_TO_RAD = Math.PI / 180d;
        public const double RAD_TO_DEGREE = 180d / Math.PI;

        /// <summary> Checks if the Euclidean distance between two Vectors is less than a specified maximum </summary>
        /// <param name="decimals"> Required accuracy after the comma </param>
        public static bool IsAlmostEqual(this Vector3 vector1, Vector3 vector2, int decimals) {
            float maxDiff = (float)Math.Pow(10, -(decimals));
            return IsAlmostEqual(vector1, vector2, maxDiff);
        }

        /// <summary> Checks if the Euclidean distance between two Vectors is less than a specified maxDiff </summary>
        public static bool IsAlmostEqual(this Vector3 vector1, Vector3 vector2, float maxDiff) {
            var distance = Vector3.Distance(vector1, vector2);
            return distance < maxDiff;
        }

        public static double AngleSignedInRadTo(this Vector3 vector1, Vector3 vector2, Vector3 axis) {
            // https://stackoverflow.com/a/33920320/165106 
            var crossProduct = vector1.CrossProduct(vector2);
            var determinant = crossProduct.DotProduct(axis);
            return Math.Atan2(determinant, vector1.DotProduct(vector2));
        }

        public static double AngleInRadTo(this Vector3 vector1, Vector3 vector2) {
            // https://stackoverflow.com/a/10145056/165106 
            var crossProduct = vector1.CrossProduct(vector2);
            var determinant = crossProduct.Length();
            return Math.Atan2(determinant, vector1.DotProduct(vector2));
        }

        public static double AngleInDegreeTo(this Vector3 vector1, Vector3 vector2) {
            return AngleInRadTo(vector1, vector2) * RAD_TO_DEGREE;
        }

        public static double DotProduct(this Vector3 vector1, Vector3 vector2) {
            return Vector3.Dot(vector1, vector2);
        }

        public static Vector3 CrossProduct(this Vector3 vector1, Vector3 vector2) {
            return Vector3.Cross(vector1, vector2);
        }

        public static Vector3 Rotate(this Vector3 input, Quaternion rotation) {
            return Vector3.Transform(input, rotation);
        }

        public static Vector3 WithLenght(this Vector3 input, float length) {
            return Vector3.Multiply(input.Normalized(), length);
        }

        public static Vector3 Normalized(this Vector3 input) {
            return Vector3.Normalize(input);
        }

        public static Vector3 Round(this Vector3 input, int digits) {
            return new Vector3((float)Math.Round(input.X, digits), (float)Math.Round(input.Y, digits), (float)Math.Round(input.Z, digits));
        }

    }

}