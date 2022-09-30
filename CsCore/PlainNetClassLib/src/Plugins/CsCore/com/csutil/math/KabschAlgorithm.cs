using System;
using System.Numerics;

namespace com.csutil.math {

    // https://en.wikipedia.org/wiki/Kabsch_algorithm
    // https://zalo.github.io/blog/kabsch/#shape-matching
    public class KabschAlgorithm {

        // Ported from https://github.com/zalo/MathUtilities/blob/master/Assets/Kabsch/Kabsch.cs 
        public class KabschSolver {

            Vector3[] QuatBasis = new Vector3[3];
            Vector3[] DataCovariance = new Vector3[3];
            Quaternion OptimalRotation = Quaternion.Identity;
            public float scaleRatio = 1f;

            public Matrix4x4 SolveKabsch(Vector3[] inPoints, Vector4[] refPoints, bool solveRotation = true, bool solveScale = false) {
                if (inPoints.Length != refPoints.Length) { return Matrix4x4.Identity; }

                //Calculate the centroid offset and construct the centroid-shifted point matrices
                Vector3 inCentroid = Vector3.Zero;
                Vector3 refCentroid = Vector3.Zero;
                float inTotal = 0f, refTotal = 0f;
                for (int i = 0; i < inPoints.Length; i++) {
                    inCentroid += new Vector3(inPoints[i].X, inPoints[i].Y, inPoints[i].Z) * refPoints[i].W;
                    inTotal += refPoints[i].W;
                    refCentroid += new Vector3(refPoints[i].X, refPoints[i].Y, refPoints[i].Z) * refPoints[i].W;
                    refTotal += refPoints[i].W;
                }
                inCentroid /= inTotal;
                refCentroid /= refTotal;

                //Calculate the scale ratio
                if (solveScale) {
                    float inScale = 0f, refScale = 0f;
                    for (int i = 0; i < inPoints.Length; i++) {
                        inScale += (new Vector3(inPoints[i].X, inPoints[i].Y, inPoints[i].Z) - inCentroid).Length();
                        refScale += (new Vector3(refPoints[i].X, refPoints[i].Y, refPoints[i].Z) - refCentroid).Length();
                    }
                    scaleRatio = (refScale / inScale);
                }

                //Calculate the 3x3 covariance matrix, and the optimal rotation
                if (solveRotation) {
                    extractRotation(TransposeMultSubtract(inPoints, refPoints, inCentroid, refCentroid, DataCovariance), ref OptimalRotation);
                }

                return UnityMath.Matrix4x4_TRS(refCentroid, Quaternion.Identity, Vector3.One * scaleRatio) *
                       UnityMath.Matrix4x4_TRS(Vector3.Zero, OptimalRotation, Vector3.One) *
                       UnityMath.Matrix4x4_TRS(-inCentroid, Quaternion.Identity, Vector3.One);
            }

            //https://animation.rwth-aachen.de/media/papers/2016-MIG-StableRotation.pdf
            //Iteratively apply torque to the basis using Cross products (in place of SVD)
            private void extractRotation(Vector3[] A, ref Quaternion q, int iterationsCount = 9) {
                var t = Log.MethodEntered("Solve Optimal Rotation");
                for (int iter = 0; iter < iterationsCount; iter++) {
                    var t2 = Log.MethodEntered("Iterate Quaternion");
                    UnityMath.FillMatrixFromQuaternion(q, ref QuatBasis);
                    Vector3 omega = (Vector3.Cross(QuatBasis[0], A[0]) +
                                     Vector3.Cross(QuatBasis[1], A[1]) +
                                     Vector3.Cross(QuatBasis[2], A[2])) *
                     (1f / Math.Abs(Vector3.Dot(QuatBasis[0], A[0]) +
                                     Vector3.Dot(QuatBasis[1], A[1]) +
                                     Vector3.Dot(QuatBasis[2], A[2]) + 0.000000001f));

                    float w = omega.Length(); // magnitude
                    if (w < 0.000000001f)
                        break;
                    q = Quaternion.CreateFromAxisAngle(omega / w, w) * q;
                    q = Quaternion.Lerp(q, q, 0f); //Normalizes the Quaternion; critical for error suppression
                    Log.MethodDone(t2);
                }
                Log.MethodDone(t);
            }

            //Calculate Covariance Matrices --------------------------------------------------
            private static Vector3[] TransposeMultSubtract(Vector3[] vec1, Vector4[] vec2, Vector3 vec1Centroid, Vector3 vec2Centroid, Vector3[] covariance) {
                var t = Log.MethodEntered("Calculate Covariance Matrix");
                for (int i = 0; i < 3; i++) { //i is the row in this matrix
                    covariance[i] = Vector3.Zero;
                }

                for (int k = 0; k < vec1.Length; k++) {//k is the column in this matrix
                    Vector3 left = (vec1[k] - vec1Centroid) * vec2[k].W;
                    Vector3 right = (new Vector3(vec2[k].X, vec2[k].Y, vec2[k].Z) - vec2Centroid) * Math.Abs(vec2[k].W);

                    covariance[0].X += left.X * right.X;
                    covariance[1].X += left.Y * right.X;
                    covariance[2].X += left.Z * right.X;
                    covariance[0].Y += left.X * right.Y;
                    covariance[1].Y += left.Y * right.Y;
                    covariance[2].Y += left.Z * right.Y;
                    covariance[0].Z += left.X * right.Z;
                    covariance[1].Z += left.Y * right.Z;
                    covariance[2].Z += left.Z * right.Z;
                }
                Log.MethodDone(t);
                return covariance;
            }

            //private static Vector3[] TransposeMultSubtract(Vector3[] vec1, Vector3[] vec2, ref Vector3[] covariance) {
            //    for (int i = 0; i < 3; i++) covariance[i] = Vector3.Zero;

            //    for (int k = 0; k < vec1.Length; k++) {//k is the column in this matrix
            //        Vector3 left = vec1[k];
            //        Vector3 right = vec2[k];

            //        covariance[0].X += left.X * right.X;
            //        covariance[1].X += left.Y * right.X;
            //        covariance[2].X += left.Z * right.X;
            //        covariance[0].Y += left.X * right.Y;
            //        covariance[1].Y += left.Y * right.Y;
            //        covariance[2].Y += left.Z * right.Y;
            //        covariance[0].Z += left.X * right.Z;
            //        covariance[1].Z += left.Y * right.Z;
            //        covariance[2].Z += left.Z * right.Z;
            //    }
            //    return covariance;
            //}
        }

    }

    // Ported methods that do exist in Unity math but not in System Numerics
    public static class UnityMath {

        public static readonly Vector3 Vector3_right = new Vector3(1, 0, 0);
        public static readonly Vector3 Vector3_up = new Vector3(0, 1, 0);
        public static readonly Vector3 Vector3_forward = new Vector3(0, 0, 1);

        // https://docs.unity3d.com/ScriptReference/Matrix4x4.GetColumn.html
        public static Vector3 GetColumn1(this Matrix4x4 m) {
            return new Vector3(m.M21, m.M22, m.M23); //return m.GetColumn(1);
        }

        // https://docs.unity3d.com/ScriptReference/Matrix4x4.GetColumn.html
        public static Vector3 GetColumn2(this Matrix4x4 m) {
            return new Vector3(m.M31, m.M32, m.M33); //return m.GetColumn(2);
        }

        // https://docs.unity3d.com/ScriptReference/Matrix4x4.GetColumn.html
        public static Vector3 GetColumn3(this Matrix4x4 m) {
            return new Vector3(m.M41, m.M42, m.M43); //return m.GetColumn(3);
        }

        public static void FillMatrixFromQuaternion(Quaternion q, ref Vector3[] covariance) {
            covariance[0] = Vector3_right.Rotate(q);
            covariance[1] = Vector3_up.Rotate(q);
            covariance[2] = Vector3_forward.Rotate(q);
        }

        public static Matrix4x4 Matrix4x4_TRS(Vector3 position, Quaternion rotation, Vector3 scale) {
            // TODO is this the correct order:
            return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
        }

        public static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float alpha) {
            return Matrix4x4_TRS(Vector3.Lerp(a.GetColumn3(), b.GetColumn3(), alpha), Quaternion.Slerp(GetQuaternion(a), GetQuaternion(b), alpha), Vector3.One);
        }

        private static Quaternion GetQuaternion(Matrix4x4 m) {
            if (m.GetColumn2() == m.GetColumn1()) { return Quaternion.Identity; }
            return Quaternion_LookRotation(m.GetColumn2(), m.GetColumn1());
        }

        private static Quaternion Quaternion_LookRotation(Vector3 forward, Vector3 upwards) {
            return Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Vector3.Zero, forward, upwards));
        }

    }

}