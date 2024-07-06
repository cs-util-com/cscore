using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using com.csutil.math;
using Xunit;

namespace com.csutil.tests {

    public class MathTests {

        private const float radToDegree = 180f / MathF.PI;
        private const float degreeToRad = MathF.PI / 180f;

        public MathTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            List<float> someNumbers = new List<float>();
            float average = 0;
            Assert.True(float.IsNaN(someNumbers.CalcMedian()));
            Assert.True(float.IsNaN(someNumbers.CalcMean()));

            average = AddValue(someNumbers, 4, average);
            Assert.Equal(4f, average);
            Assert.Equal(4f, someNumbers.CalcMedian());
            average = AddValue(someNumbers, 6, average);
            Assert.Equal(5f, average);
            Assert.Equal(4f, someNumbers.CalcMedian());
            average = AddValue(someNumbers, 8, average);
            Assert.Equal(6f, average);
            Assert.Equal(6f, someNumbers.CalcMedian());

            // Mean and Median are also usable with Vector3:
            var someVectors = new List<Vector3>();
            someVectors.Add(new Vector3(0, 0, 0));
            someVectors.Add(new Vector3(2, 2, 2));
            someVectors.Add(new Vector3(4, 4, 4));
            Assert.Equal(new Vector3(2, 2, 2), someVectors.CalcMean());
            Assert.Equal(new Vector3(2, 2, 2), someVectors.CalcMedian());
            someVectors.Add(new Vector3(999, 999, 999));
            Assert.Equal(new Vector3(2, 2, 2), someVectors.CalcMedian());
            Assert.NotEqual(new Vector3(2, 2, 2), someVectors.CalcMean());
            someVectors.Add(new Vector3(999, 999, 999));
            Assert.Equal(new Vector3(4, 4, 4), someVectors.CalcMedian());

        }

        private static float AddValue(List<float> self, float newValue, float oldAverage) {
            var newAverage = CalculateAverage.CalcRunningMean(oldAverage, newValue, self.Count);
            self.Add(newValue);
            return newAverage;
        }

        [Fact]
        public void TestMathEvaluator() {
            Assert.Equal(9, Numbers.Calculate("1 + 2 * 4"));
            Assert.Equal(1, Numbers.Calculate("(1 + 2 * 4) % 2"));
            Assert.Equal(4.5, Numbers.Calculate("(1 + 2 * 4) / 2"));
            Assert.Equal(1, Numbers.Calculate("0.5 + 0.5"));
            Assert.Equal(1, Numbers.Calculate("0.5 + 0,5"));
            Assert.Equal(1, Numbers.Calculate("0,5 + 0,5"));
            Assert.Equal(0.6667, Numbers.Calculate("2/3"), 4);
            Assert.NotEqual(0.667, Numbers.Calculate("2/3"), 4);
        }

        [Fact]
        public static void TestDataTable() {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("A", typeof(int));
            dt.Columns.Add("B", typeof(int));
            dt.Rows.Add(11, 12); // Insert a row with A=4, B=1
            // Querying the table for specific entries:
            var boolResult = dt.Select("A>B-2").Length > 0;
            Assert.True(boolResult); // 11 > 12-2
            // Add a result column that calculates a formula based on the entries:
            var columnName = "Result Column";
            dt.Columns.Add(columnName, typeof(int), "A+B*2");
            var rowNr = 0;
            var valResult = dt.Rows[rowNr][columnName];
            Assert.Equal(35, valResult); // 11 + 12*2  = 35
        }

        [Fact]
        public static void TestColorMath() {

            AssertEqualRgb("[1, 0, 0]", ColorMath.RgbToHsv(1, 0, 0));
            AssertEqualRgb("[0, 1, 0]", ColorMath.RgbToHsv(0, 1, 0));
            AssertEqualRgb("[0, 0, 1]", ColorMath.RgbToHsv(0, 0, 1));

            Assert.Equal(0, ColorMath.CalcBrightness(0, 0, 0));
            Assert.Equal(0.9278, ColorMath.CalcBrightness(1, 1, 0)); // Yellow is bright
            Assert.Equal(1, ColorMath.CalcBrightness(1, 1, 1));

            var whiteBrightnes = ColorMath.CalcBrightness(1, 1, 1);
            var blueBrightness = ColorMath.CalcBrightness(0, 0, 1);
            Assert.Equal(1, whiteBrightnes);
            Assert.Equal(0.0722, blueBrightness); // Blue is dark
            // Blue on white has a great contrast (is well readable):
            Assert.True(8 < ColorMath.CalcContrastRatio(whiteBrightnes, blueBrightness));

            var green = ColorMath.RgbToHsv(0, 1, 0);
            Assert.Equal("[0.3333333, 1, 1]", green.ToStringV2(c => c.ToStringV2()));
            ColorMath.InvertHue(green); // Green becomes purple
            Assert.Equal("[0.8333333, 1, 1]", green.ToStringV2(c => c.ToStringV2()));
            AssertEqualRgb("[1, 0, 1]", green); // Purple is red + blue


        }

        private static void AssertEqualRgb(string rgb, float[] hsvColor) {
            var redRgb = ColorMath.HsvToRgb(hsvColor[0], hsvColor[1], hsvColor[2]);
            Assert.Equal(rgb, redRgb.ToStringV2(c => c.ToStringV2()));
        }


        [Fact]
        public static void VectorRotationTest1() {

            { // Test rotation in circles
                var v1 = new Vector3(2, 0, 0);
                var v2 = v1.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 90 * degreeToRad)).Round(4);
                Assert.Equal(new Vector3(0, 0, -2), v2);
                Assert.Equal(90, v2.AngleInDegreeTo(v1));
                Assert.Equal(90, v1.AngleInDegreeTo(v2));

                Assert.Equal(-90, Math.Round(v2.AngleSignedInRadTo(v1, Vector3.UnitY) * radToDegree, 4));
                Assert.Equal(90, Math.Round(v1.AngleSignedInRadTo(v2, Vector3.UnitY) * radToDegree, 4));

                var v3 = v2.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 90 * degreeToRad)).Round(4);
                Assert.Equal(new Vector3(-2, 0, 0), v3);
                Assert.Equal(90, v3.AngleInDegreeTo(v2));
                Assert.Equal(180, v3.AngleInDegreeTo(v1));

                Assert.Equal(180, Math.Round(v3.AngleSignedInRadTo(v1, Vector3.UnitY) * radToDegree, 4));
                Assert.Equal(180, Math.Round(v1.AngleSignedInRadTo(v3, Vector3.UnitY) * radToDegree, 4));

                var v4 = v3.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 90 * degreeToRad)).Round(4);
                Assert.Equal(new Vector3(0, 0, 2), v4);
                Assert.Equal(90, v4.AngleInDegreeTo(v3));

                Assert.Equal(90, Math.Round(v4.AngleSignedInRadTo(v1, Vector3.UnitY) * radToDegree, 4));
                Assert.Equal(-90, Math.Round(v1.AngleSignedInRadTo(v4, Vector3.UnitY) * radToDegree, 4));

                var v1_2 = v4.Rotate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, 90 * degreeToRad)).Round(4);
                Assert.Equal(v1, v1_2);
            }

        }

        [Fact]
        public static void TestGetRotationDelta() {
            TestGetRotationDeltaWith(80, 100);
            TestGetRotationDeltaWith(100, 80);
            TestGetRotationDeltaWith(350, 10);
            TestGetRotationDeltaWith(10, 350);

            // Generate 100k random angles and test if the diff is correct:
            var rnd = new Random();
            for (int i = 0; i < 100000; i++) {
                var angle1 = rnd.NextFloat(0, 360);
                var angle2 = rnd.NextFloat(0, 360);
                TestGetRotationDeltaWith(angle1, angle2);
            }
        }

        private static void TestGetRotationDeltaWith(float angle1InDegree, float angle2InDegree) {
            var q1 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle1InDegree * degreeToRad);
            var q2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle2InDegree * degreeToRad);
            var delta = q1.GetRotationDeltaTo(q2);

            // Now rotate a vector by this diff and check if the signed angle between the 2 vectors makes sense:
            var expectedAngleInDegree = (angle2InDegree - angle1InDegree + 360d) % 360d;
            var a = Vector3.UnitX;
            var b = a.Rotate(delta);
            var actualAngleInDegree = a.AngleSignedInRadTo(b, Vector3.UnitY) * radToDegree;
            actualAngleInDegree = (actualAngleInDegree + 360d) % 360d;
            Assert_AlmostEqual(expectedAngleInDegree, actualAngleInDegree);

            // Compare also with the result of GetRotationDeltaInDegreeTo:
            var deltaAngleInDegree = q1.GetRotationDeltaInDegreeTo(q2);
            if (expectedAngleInDegree > 180) { expectedAngleInDegree = 360 - expectedAngleInDegree; }
            if (actualAngleInDegree > 180) { actualAngleInDegree = 360 - actualAngleInDegree; }
            Assert_AlmostEqual(expectedAngleInDegree, deltaAngleInDegree);
            Assert_AlmostEqual(actualAngleInDegree, deltaAngleInDegree);
        }

        private static void Assert_AlmostEqual(double a, double b, double maxDelta = 0.1) {
            var diff = Math.Abs(a - b);
            Assert.True(diff < maxDelta, $"Expected {a} to be almost equal to {b} but diff was {diff}");
        }

        [Fact]
        public void TestIsAlmostEqual() {

            var vec1 = new Vector3(1, 1, 3);
            var vec2 = new Vector3(1, 1, 3.01f);

            Assert.True(vec1.IsAlmostEqual(vec2, 2));
            Assert.False(vec1.IsAlmostEqual(vec2, 3));
            Assert.True(vec1.IsAlmostEqual(vec2, 0.01f));
            Assert.False(vec1.IsAlmostEqual(vec2, 0.009f));

        }

        [Fact]
        public void TestMatrixComposeDecompose() {

            var translation = new Vector3(1, 2, 3);
            var rotation = Quaternion.CreateFromYawPitchRoll(15 * degreeToRad, 45 * degreeToRad, 75 * degreeToRad);
            var scale = new Vector3(7, 6, 5);

            var matrix = Matrix4x4Extensions.Compose(translation, rotation, scale);

            var success = matrix.Decompose(out var scale2, out var rotation2, out var translation2);
            Assert.True(success);
            var digits = 6;
            Assert.True(translation.IsAlmostEqual(translation2, decimals: digits));
            Assert.True(rotation.Equals(rotation2, digits: digits));
            Assert.True(scale.IsAlmostEqual(scale2, decimals: digits));

        }

        [Fact]
        public void TestBezierTrajectoryDeformation() {
            List<Matrix4x4> trajectory = new List<Matrix4x4>();
            // All individual points are titled 45 degree but are they are all on a straight line so this rotation does not affect the trajectory
            var inputRot = Quaternion.CreateFromYawPitchRoll(0, 45 * degreeToRad, 0);
            trajectory.Add(Matrix4x4Extensions.Compose(Vector3.Zero, inputRot, Vector3.One));
            trajectory.Add(Matrix4x4Extensions.Compose(new Vector3(0.5f, 0.5f, 0.5f), inputRot, Vector3.One));
            trajectory.Add(Matrix4x4Extensions.Compose(new Vector3(1, 1, 1), inputRot, Vector3.One));
            trajectory.Add(Matrix4x4Extensions.Compose(new Vector3(1.5f, 1.5f, 1.5f), inputRot, Vector3.One));
            var endPose = Matrix4x4Extensions.Compose(new Vector3(2, 2, 2), inputRot, Vector3.One);
            trajectory.Add(endPose);

            // Moving the last point to 4,4,4 would be a pure stretch transformation that is easy to assert below
            var correctedEndPos = new Vector3(4, 4, 4);
            var correctedEndPose = Matrix4x4Extensions.Compose(correctedEndPos, inputRot, Vector3.One);
            var corrected = trajectory.CalcBezierTrajectoryDeformation(correctedEndPose).ToList();

            { // First point should not be changed
                Assert.Equal(trajectory.First(), corrected.First());
            }
            { // The second point should be double the distance from zero now:
                corrected.Skip(1).First().Decompose(out var scale, out var rotation, out var translation);
                Assert.True(translation.IsAlmostEqual(new Vector3(1, 1, 1), 4));
                Assert.True(rotation.GetRotationDeltaTo(inputRot).GetEulerAnglesAsPitchYawRoll().IsAlmostEqual(Vector3.Zero, 4));
                Assert.True(scale.IsAlmostEqual(Vector3.One, decimals: 4));
            }
            { // The third point should be double the distance from zero now:
                corrected.Skip(2).First().Decompose(out var scale, out var rotation, out var translation);
                Assert.True(translation.IsAlmostEqual(new Vector3(2, 2, 2), 4));
                Assert.True(rotation.GetRotationDeltaTo(inputRot).GetEulerAnglesAsPitchYawRoll().IsAlmostEqual(Vector3.Zero, 4));
                Assert.True(scale.IsAlmostEqual(Vector3.One, decimals: 4));
            }
            { // The fourth point should be double the distance from zero now:
                corrected.Skip(3).First().Decompose(out var scale, out var rotation, out var translation);
                Assert.True(translation.IsAlmostEqual(new Vector3(3, 3, 3), 4));
                Assert.True(rotation.GetRotationDeltaTo(inputRot).GetEulerAnglesAsPitchYawRoll().IsAlmostEqual(Vector3.Zero, 4));
                Assert.True(scale.IsAlmostEqual(Vector3.One, decimals: 4));
            }
            { // The last point should be the corrected end pose:
                corrected.Last().Decompose(out var scale, out var rotation, out var translation);
                Assert.True(rotation.GetRotationDeltaTo(inputRot).GetEulerAnglesAsPitchYawRoll().IsAlmostEqual(Vector3.Zero, 4));
                Assert.True(scale.IsAlmostEqual(Vector3.One, decimals: 4));
                Assert.True(translation.IsAlmostEqual(correctedEndPos, 4), $"Expected {correctedEndPos} but got {translation}");
            }

        }

        [Fact]
        public void ExampleUsageOfEulerAngles() {
            {
                var yaw = 180f;
                var rotation = Quaternion.CreateFromYawPitchRoll(yaw * degreeToRad, 0, 0);
                var eulerAngles = rotation.GetEulerAnglesAsPitchYawRoll();
                Assert.Equal(new Vector3(0, yaw, 0), eulerAngles);

                // Rotating the x axis by 180 degree results in the x axis pointing backwards:
                var vector = Vector3.UnitX;
                var rotated = rotation.Rotate(vector);
                Assert.Equal(vector * -1, rotated);

                Log.d($"Rotating the vector {vector} by {eulerAngles.Y} degree results in {rotated}");
            }
            {
                var pitch = 90f;
                var rotation = Quaternion.CreateFromYawPitchRoll(0, pitch * degreeToRad, 0);
                var eulerAngles = rotation.GetEulerAnglesAsPitchYawRoll();
                Assert.Equal(new Vector3(pitch, 0, 0), eulerAngles);

                // Rotating the Up vector forward by 90 degree results in the Forward vector:
                var vector = Vector3.UnitY;
                var rotated = rotation.Rotate(vector);
                Assert.Equal(Vector3.UnitZ, rotated);

                Log.d($"Rotating the vector {vector} forward by {eulerAngles.X} degree results in {rotated}");
            }
        }

        [Fact]
        public void TestGetEulerAngles1() {
            TestGetEulerAnglesWith(90.000015f, -159.58305f, 140.32559f);
            TestGetEulerAnglesWith(89.99931f, -132.24231f, 113.949486f);
            TestGetEulerAnglesWith(61, 111, 87);
            TestGetEulerAnglesWith(90.012695f, -73.464935f, 157.1788f);
            TestGetEulerAnglesWith(90.004265f, 21.351936f, 15.406252f);
            TestGetEulerAnglesWith(90.0001f, -153.36794f, 50.995544f);

            var rnd = new Random();
            for (int i = 0; i < 10000; i++) {
                // Generate different angles in degrees for TestGetEulerAngles2: 
                var pitch = rnd.NextFloat(-180, 180);
                var yaw = rnd.NextFloat(-180, 180);
                var roll = rnd.NextFloat(-180, 180);
                TestGetEulerAnglesWith(pitch, yaw, roll);
            }
            TestGetEulerAnglesWith(45, 15, 75);
        }

        [Fact]
        public void TestGetEulerAngles2() {
            TestGetEulerAnglesWith(90, -139, 127);
            TestGetEulerAnglesWith(0, -180, -39);
            TestGetEulerAnglesWith(90, -46, 13);
            TestGetEulerAnglesWith(-90, 130, 158);
            TestGetEulerAnglesWith(90, -69, -74);
            var rnd = new Random();
            for (int i = 0; i < 10000; i++) {
                // Generate different angles in degrees for TestGetEulerAngles2: 
                var pitch = rnd.Next(-180, 180);
                var yaw = rnd.Next(-180, 180);
                var roll = rnd.Next(-180, 180);
                TestGetEulerAnglesWith(pitch, yaw, roll);
            }
        }

        private static void TestGetEulerAnglesWith(float pitch, float yaw, float roll) {
            var inputEulers = new Vector3(pitch, yaw, roll);
            var rotation = Quaternion.CreateFromYawPitchRoll(yaw * degreeToRad, pitch * degreeToRad, roll * degreeToRad);
            var eulers = rotation.GetEulerAnglesAsPitchYawRoll();
            var rotation2 = Quaternion.CreateFromYawPitchRoll(eulers.Y * degreeToRad, eulers.X * degreeToRad, eulers.Z * degreeToRad);
            var eulers2 = rotation2.GetEulerAnglesAsPitchYawRoll();
            var diff = rotation.GetRotationDeltaTo(rotation2);

            // Inverting the quaternion is the same rotation
            if (Math.Sign(diff.W) == -1) { diff = -diff; }

            Assert.True(diff.Equals(Quaternion.Identity, digits: 1),
                $"diff={diff} from \n rotation ={rotation} (eulers={inputEulers}) to \n rotation2={rotation2} (eulers={eulers2})");
            // Assert.True(eulers.IsSimilarTo(inputEulers, digits: digits), $"Eulers: {eulers} != {inputEulers}");
            // Assert.True(eulers.IsSimilarTo(eulers2, digits: digits), $"Eulers: {eulers} != {eulers2}");
        }

        public class WeightedMedianTests {

            public WeightedMedianTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

            [Fact]
            public void ExampleUsage1() {
                IEnumerable<Element> elements = new List<Element>() {
                    new Element { Value = 80, Weight = 10 },
                    new Element { Value = 90, Weight = 10 },
                    new Element { Value = 95, Weight = 10 }, // Should be the weighted median
                    new Element { Value = 100, Weight = 10 },
                    new Element { Value = 9999, Weight = 10 }, // Outlier that would impact the mean but not the median
                    new Element { Value = 0, Weight = .1 },
                    new Element { Value = 1, Weight = .1 },
                    new Element { Value = 2, Weight = .1 },
                    new Element { Value = 3, Weight = .1 },
                    new Element { Value = 4, Weight = .1 },
                    new Element { Value = 5, Weight = .1 },
                };
                elements = new Random().ShuffleEntries(elements);
                var weightedMedian = elements.CalcWeightedMedian(x => x.Weight);
                Assert.Equal(95, weightedMedian.Item1.Value);
                Assert.Equal(95, weightedMedian.Item2.Value); // Uneven nr of elems, so both results are the same
                // Calculating the normal nedian will not result in the correct value:
                Assert.Equal(5, elements.CalcMedian(x => x.Value));
            }

            [Fact]
            public void ExampleUsage2() {
                IEnumerable<Element> elements = new List<Element>() {
                    new Element { Value = 90, Weight = 10 },
                    new Element { Value = 100, Weight = 10 },
                };
                elements = new Random().ShuffleEntries(elements);
                var weightedMedian = elements.CalcWeightedMedian(x => x.Weight);
                // Since for an even number of elems both median candidates are returned an average can be caluclated on these:
                Assert.Equal(95, (weightedMedian.Item1.Value + weightedMedian.Item2.Value) / 2);
            }

            private class Element : IComparable {
                public double Value { get; set; }
                public double Weight { get; set; }

                public int CompareTo(object obj) {
                    if (obj is Element other) {
                        if (Value == other.Value) { return 0; }
                        return Value < other.Value ? -1 : 1;
                    }
                    throw new InvalidOperationException("Can only compare to other instances of Element");
                }

            }

            [Fact]
            public void TestMatrix4x4GetAndSet() {
                var m = Matrix4x4.Identity;
                Assert.Equal(1, m.Get(0, 0));
                m.Set(0, 0, 2);
                Assert.Equal(2, m.Get(0, 0));
                var row = m.GetRow(0);
                Assert.Equal(new Vector4(2, 0, 0, 0), row);
            }

            [Fact]
            public void TestMatrix4x4SetPositionRotationScale() {
                var matrix = Matrix4x4.Identity;
                var testPostion = new Vector3(1, 2, 3);
                var testRotation = Quaternion.CreateFromYawPitchRoll(45 * degreeToRad, 95 * degreeToRad, 110 * degreeToRad);
                var testScale = new Vector3(4, 4, 4);
                
                var newMatrix = matrix.WithPosition(testPostion).WithRotation(testRotation).WithScale(testScale);
                Assert.Equal(Matrix4x4.Identity, matrix); // The old matrix m is not changed
                
                // The new matrix has the expected values:
                newMatrix.Decompose(out var scale, out var rotation, out var translation);
                Assert.Equal(testPostion, translation);
                var rotDelta = testRotation.GetRotationDeltaInDegreeTo(rotation);
                Assert.True(rotDelta < 0.1, $"rotDelta={rotDelta}");
                Assert.Equal(testScale, scale);
            }

            [Fact]
            public void TestCurveFitting1() {

                // Points from a simple y=x^2 function:
                var points = new List<Vector2>() {
                    new Vector2(0, 0),
                    new Vector2(1, 1),
                    new Vector2(2, 4),
                    new Vector2(3, 9),
                    new Vector2(4, 16),
                    new Vector2(5, 25),
                    new Vector2(10, 100),
                };

                var coefficients = CurveFitting.CalcPolynomialCoefficients(points, coefficientCount: 5);
                // The resulting formula should be: y = 0 + 0*x + 1*x^2 + 0*x^3 + 0*x^4 :
                Assert.Equal(new double[] { 0, 0, 1, 0, 0 }, coefficients);
                Assert.Equal("var y = Math.Pow(x, 2);", CurveFitting.GetPolynomialStringFor(coefficients));

                // Check that for each point the formula returns the correct y value:
                foreach (var p in points) {
                    var y = CurveFitting.CalcPolynomialFor(coefficients, p.X);
                    Assert.Equal(p.Y, y);
                }

            }

            [Fact]
            public void TestCurveFitting2() {

                // Points from a simple y=x^2 function:
                var points = new List<Vector2>() {
                    new Vector2(0, 0),
                    new Vector2(1, 1),
                    new Vector2(2, 4),
                    new Vector2(3, 9),
                    new Vector2(4, 16),
                    new Vector2(5, 25),
                    new Vector2(10, 999), // One outlier that should be ignored
                };

                var bestFunction = CurveFitting.CalcAllPolynomialFunctionsWithRansac(points, new Random(Seed: 1), inlierDistance: 5);
                Assert.True(new double[] { 0, 0, 1 }.SequenceEqual(bestFunction.Coefficients), $"actual func={bestFunction}");
                Assert.Equal("var y = Math.Pow(x, 2);", bestFunction.ToString());

            }


        }

    }

}