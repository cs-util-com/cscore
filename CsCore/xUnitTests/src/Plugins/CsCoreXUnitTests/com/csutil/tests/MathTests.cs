using System.Collections.Generic;
using com.csutil.math;
using Xunit;

namespace com.csutil.tests {
    public class MathTests {

        public MathTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {

            List<float> someNumbers = new List<float>();
            float average = 0;

            average = AddValue(someNumbers, 4, average);
            Assert.Equal(4f, average);
            average = AddValue(someNumbers, 6, average);
            Assert.Equal(5f, average);
            average = AddValue(someNumbers, 8, average);
            Assert.Equal(6f, average);

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
            Assert.Equal(1, Numbers.Calculate("0.5 + 0,5"));
        }

    }

}