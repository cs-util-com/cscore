using System.Linq;
using System.Numerics;
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

    }
    
}