using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using com.csutil.algorithms;
using com.csutil.math;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class KabschTests {

        public KabschTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void TestKabschAlgorithm() {

            var input = new Vector3[] {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(2, 0, 0),
                new Vector3(2, 0, -1),
            };
            var dataToAlignTo = new Vector4[] {
                new Vector4(0, 0, 0, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 2, 1),
                new Vector4(1, 0, 2, 1),
            };
            var solver = new KabschAlgorithm();
            var alignmentResult = solver.SolveKabsch(input, dataToAlignTo);

            var dataToAlignTo2 = dataToAlignTo.Map(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
            var output = input.Map(v => Vector3.Transform(v, alignmentResult)).ToArray();

            AssertAreEqual(dataToAlignTo2[0], (output[0]));
            AssertAreEqual(dataToAlignTo2[1], (output[1]));
            AssertAreEqual(dataToAlignTo2[2], (output[2]));
            AssertAreEqual(dataToAlignTo2[3], (output[3]));

            alignmentResult.Decompose(out var scale, out var rotation, out var translation);

            Assert.True(translation.IsSimilarTo(Vector3.Zero, 6));
            Assert.True(scale.IsSimilarTo(Vector3.One, 6));

        }

        private static void AssertAreEqual(Vector3 a, Vector3 b, double allowedDelta = 0.00001) {
            var diff = a - b;
            Assert.True(diff.Length() < allowedDelta, "diff=" + diff);
        }

        [Fact]
        public void TestKabschPlusRansacAlgorithm1() {
            var pairs = new List<Tuple<Vector3, Vector4>>();
            {
                var input = new Vector3[] {
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(2, 0, 0),
                    new Vector3(3, 0, 0),
                };
                var dataToAlignTo = new Vector4[] {
                    new Vector4(0, 0, 0, 1),
                    new Vector4(0, 0, 1, 1),
                    new Vector4(0, 0, 2, 1),
                    new Vector4(0, 0, 3, 1),
                };
                for (int i = 0; i < input.Count(); i++) {
                    pairs.Add(new Tuple<Vector3, Vector4>(input[i], dataToAlignTo[i]));
                }
            }

            var tollerance = 1;
            var result = new Random().RunRansac(pairs, 1, subsetOfAllPairs => {
                var result = SolveKabschFor(subsetOfAllPairs);
                // Since all points are perfect inliers the meanAlignmentError should always be 0 for any random subset of points:
                Assert.Equal(0, result.meanAlignmentError, precision: 10);
                return result;
            }, (model, ele) => {
                return CalcError(ele.Item2, Vector3.Transform(ele.Item1, model.alignmentMatrix)) < model.meanAlignmentError + tollerance;
            });

            Assert.Equal(4, result.inliers.Count());

            result.alignmentMatrix.Decompose(out var scale, out var rotation, out var translation);
            Assert.True(translation.IsSimilarTo(Vector3.Zero, 6));
            Assert.True(scale.IsSimilarTo(Vector3.One, 6));

            {
                var dataToAlignTo = result.inliers.Map(x => x.Item2).ToList();
                var dataToAlignTo2 = dataToAlignTo.Map(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
                var output = result.alignedPoints;
                AssertAreEqual(dataToAlignTo2[0], (output[0]));
                AssertAreEqual(dataToAlignTo2[1], (output[1]));
                AssertAreEqual(dataToAlignTo2[2], (output[2]));
            }

        }

        [Fact]
        public void TestKabschPlusRansacAlgorithm2() {
            var allPairs = new List<Tuple<Vector3, Vector4>>();
            {
                var input = new Vector3[] {
                    new Vector3(-2, 0, 0),
                    new Vector3(-1, 0, 0),
                    new Vector3(0, 0, 0),
                    new Vector3(1, 0, 0),
                    new Vector3(2, 0, 0),
                    new Vector3(3, 0, 0),
                };
                var dataToAlignTo = new Vector4[] {
                    new Vector4(0, 0, -2, 1),
                    new Vector4(0, 0, -1, 1),
                    new Vector4(0, 0, 0, 1),
                    new Vector4(0, 0, 1, 1),
                    new Vector4(0, 0, 2, 1),
                    new Vector4(99, 0, 3, 1), // Outlier that ransac should filter out
                };
                for (int i = 0; i < input.Count(); i++) {
                    allPairs.Add(new Tuple<Vector3, Vector4>(input[i], dataToAlignTo[i]));
                }
            }

            var tollerance = 1;
            var result = new Random().RunRansac(allPairs, 1, subsetOfAllPairs => {
                return SolveKabschFor(subsetOfAllPairs);
            }, (model, ele) => {
                return CalcError(ele.Item2, Vector3.Transform(ele.Item1, model.alignmentMatrix)) < model.meanAlignmentError + tollerance;
            });

            Assert.Equal(allPairs.Count() - 1, result.inliers.Count());
            Assert.Equal(1, result.outliers.Count);
            Assert.Equal(new Vector4(99, 0, 3, 1), result.outliers.Single().Item2);

            result.alignmentMatrix.Decompose(out var scale, out var rotation, out var translation);
            Assert.True(translation.IsSimilarTo(Vector3.Zero, 6), "translation=" + translation);
            Assert.True(scale.IsSimilarTo(Vector3.One, 6), "scale=" + scale);

            {
                var dataToAlignTo = result.inliers.Map(x => x.Item2).ToList();
                var dataToAlignTo2 = dataToAlignTo.Map(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
                var output = result.alignedPoints;
                AssertAreEqual(dataToAlignTo2[0], (output[0]));
                AssertAreEqual(dataToAlignTo2[1], (output[1]));
                AssertAreEqual(dataToAlignTo2[2], (output[2]));
                AssertAreEqual(dataToAlignTo2[3], (output[3]));
            }

            // Show that if the outlier would not be filtered out the result would be much worse:
            var modelForAllPointsIncludingOutlier = SolveKabschFor(allPairs);
            Assert.True(modelForAllPointsIncludingOutlier.meanAlignmentError > result.meanAlignmentError * 100);
        }

        private static KabschResult SolveKabschFor(IEnumerable<Tuple<Vector3, Vector4>> subsetOfAllPairs) {
            var input = subsetOfAllPairs.Map(x => x.Item1).ToList();
            var dataToAlignTo = subsetOfAllPairs.Map(x => x.Item2).ToList();
            var alignmentResult = new KabschAlgorithm().SolveKabsch(input, dataToAlignTo);
            var alignedPoints = input.Map(v => Vector3.Transform(v, alignmentResult)).ToArray();
            var errors = new List<double>();
            for (int i = 0; i < dataToAlignTo.Count; i++) { errors.Add(CalcError(dataToAlignTo[i], alignedPoints[i])); }
            return new KabschResult {
                alignmentMatrix = alignmentResult,
                alignedPoints = alignedPoints,
                meanAlignmentError = errors.CalcMean(),
                totalModelError = errors.Sum()
            };
        }

        private class KabschResult : Ransac.IModel<Tuple<Vector3, Vector4>> {
            public Matrix4x4 alignmentMatrix { get; set; }
            public Vector3[] alignedPoints { get; set; }
            public double? totalModelError { get; set; }
            public double meanAlignmentError { get; set; }
            public IEnumerable<Tuple<Vector3, Vector4>> inliers { get; set; }
            public List<Tuple<Vector3, Vector4>> outliers { get; set; }
        }

        private static float CalcError(Vector4 a, Vector3 b) {
            return (ToVec3(a) - b).LengthSquared();
        }

        private static Vector3 ToVec3(Vector4 x) { return new Vector3(x.X, x.Y, x.Z); }

    }

}