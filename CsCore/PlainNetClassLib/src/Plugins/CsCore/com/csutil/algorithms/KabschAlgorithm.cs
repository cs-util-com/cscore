using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace com.csutil.math {

    /// <summary>
    /// https://en.wikipedia.org/wiki/Kabsch_algorithm
    /// https://zalo.github.io/blog/kabsch/#shape-matching
    /// http://nghiaho.com/?page_id=671
    /// Ported from https://github.com/zalo/MathUtilities/blob/master/Assets/Kabsch/Kabsch.cs 
    /// </summary>
    public class KabschAlgorithm {

        private Quaternion _optimalRotation = Quaternion.Identity;
        public readonly OptimalRotationViaPolarDecompositionSolver optimalRotationSolver = new OptimalRotationViaPolarDecompositionSolver();

        /// <summary> </summary>
        /// <param name="inPoints"></param>
        /// <param name="refPoints"> the 4th value is the weight of the point for calculating the centroid </param>
        /// <param name="solveRotation"></param>
        /// <param name="solveScale"></param>
        /// <returns></returns>
        public Matrix4x4 SolveKabsch(IReadOnlyList<Vector3> inPoints, IReadOnlyList<Vector4> refPoints, bool solveRotation = true, bool solveScale = false) {
            var inPointsCount = inPoints.Count;
            if (inPointsCount != refPoints.Count) { throw new InvalidDataException("Length of the point lists was not equal"); }

            //Calculate the centroid offset and construct the centroid-shifted point matrices
            Vector3 inCentroid = Vector3.Zero;
            Vector3 refCentroid = Vector3.Zero;
            float inTotal = 0f, refTotal = 0f;
            for (int i = 0; i < inPointsCount; i++) {
                inCentroid += new Vector3(inPoints[i].X, inPoints[i].Y, inPoints[i].Z) * refPoints[i].W;
                inTotal += refPoints[i].W;
                refCentroid += new Vector3(refPoints[i].X, refPoints[i].Y, refPoints[i].Z) * refPoints[i].W;
                refTotal += refPoints[i].W;
            }
            inCentroid /= inTotal;
            refCentroid /= refTotal;

            //Calculate the scale ratio
            float scaleRatio = 1f;
            if (solveScale) {
                float inScale = 0f, refScale = 0f;
                for (int i = 0; i < inPointsCount; i++) {
                    inScale += (new Vector3(inPoints[i].X, inPoints[i].Y, inPoints[i].Z) - inCentroid).Length();
                    refScale += (new Vector3(refPoints[i].X, refPoints[i].Y, refPoints[i].Z) - refCentroid).Length();
                }
                scaleRatio = (refScale / inScale);
            }

            //Calculate the 3x3 covariance matrix, and the optimal rotation
            if (solveRotation) {
                optimalRotationSolver.Solve(inPoints, refPoints, inCentroid, refCentroid, ref _optimalRotation);
            }

            var translation = Matrix4x4.CreateTranslation(-inCentroid);
            var rotation = Matrix4x4.CreateFromQuaternion(_optimalRotation);
            var scale = Matrix4x4Extensions.Compose(refCentroid, Quaternion.Identity, Vector3.One * scaleRatio);
            return translation * rotation * scale;
        }

    }

    public class OptimalRotationViaSVDSolver {
        // TODO implement to compare against PolarDecompositionSolver
    }

    public class OptimalRotationViaPolarDecompositionSolver {

        private const float TWO_PI = 2f * 3.1415926535897931f;
        private static readonly Vector3 Vector3_right = new Vector3(1, 0, 0);
        private static readonly Vector3 Vector3_up = new Vector3(0, 1, 0);
        private static readonly Vector3 Vector3_forward = new Vector3(0, 0, 1);

        // Vars used for caching to avoid garbage:
        private readonly Vector3[] _dataCovariance = new Vector3[3];
        private Vector3[] _quatBasis = new Vector3[3];

        public bool ignoreYAxisForRotationSolving = false;

        public void Solve(IReadOnlyList<Vector3> inPoints, IReadOnlyList<Vector4> refPoints, Vector3 inCentroid, Vector3 refCentroid, ref Quaternion resultOptimalRotation, int iterationsCount = 9) {
            ExtractRotation(TransposeMultSubtract(inPoints, refPoints, inCentroid, refCentroid, _dataCovariance), ref resultOptimalRotation, iterationsCount);
        }

        /// <summary> Iteratively apply torque to the basis using Cross products (in place of SVD)
        /// Uses Matthias Muller's polar decomposition solver in place of SVD
        /// https://animation.rwth-aachen.de/media/papers/2016-MIG-StableRotation.pdf </summary>
        private void ExtractRotation(Vector3[] A, ref Quaternion resultRotQ, int iterationsCount) {
            for (int iter = 0; iter < iterationsCount; iter++) {
                FillMatrixFromQuaternion(resultRotQ, ref _quatBasis);
                Vector3 omega = (Vector3.Cross(_quatBasis[0], A[0]) +
                    Vector3.Cross(_quatBasis[1], A[1]) +
                    Vector3.Cross(_quatBasis[2], A[2])) *
                    (1f / Math.Abs(Vector3.Dot(_quatBasis[0], A[0]) +
                        Vector3.Dot(_quatBasis[1], A[1]) +
                        Vector3.Dot(_quatBasis[2], A[2]) + 0.000000001f));

                float w = omega.Length(); // magnitude
                if (w < 0.000000001f) { break; }
                var axis = omega / w;
                var angle = w % TWO_PI;
                resultRotQ = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * resultRotQ);
            }
        }

        /// <summary> Calculate Covariance Matrices </summary>
        private Vector3[] TransposeMultSubtract(IReadOnlyList<Vector3> vectors1, IReadOnlyList<Vector4> vectors2, Vector3 vec1Centroid, Vector3 vec2Centroid, Vector3[] covariance) {
            for (var i = 0; i < 3; i++) { //i is the row in this matrix
                covariance[i] = Vector3.Zero;
            }

            if (ignoreYAxisForRotationSolving) {
                vec1Centroid.Y = 0;
                vec2Centroid.Y = 0;
            }

            for (var k = 0; k < vectors1.Count; k++) { //k is the column in this matrix
                var vector1 = vectors1[k];
                var vector2 = vectors2[k];
                if (ignoreYAxisForRotationSolving) {
                    vector1.Y = 0;
                    vector2.Y = 0;
                }
                Vector3 left = (vector1 - vec1Centroid) * vector2.W;
                Vector3 right = (new Vector3(vector2.X, vector2.Y, vector2.Z) - vec2Centroid) * Math.Abs(vector2.W);
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
            return covariance;
        }

        private static void FillMatrixFromQuaternion(Quaternion q, ref Vector3[] covariance) {
            covariance[0] = Vector3_right.Rotate(q);
            covariance[1] = Vector3_up.Rotate(q);
            covariance[2] = Vector3_forward.Rotate(q);
        }

    }

}