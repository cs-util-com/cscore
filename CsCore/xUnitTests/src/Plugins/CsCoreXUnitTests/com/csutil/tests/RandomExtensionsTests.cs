using System;
using Xunit;
using com.csutil.random;
using System.Collections.Generic;

namespace com.csutil.tests.random {

    public class RandomExtensionsTests {

        public RandomExtensionsTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void RandomExtensions_Examples() {

            var random = new Random();

            { // Example for random.NextBool:
                int heads = 0;
                int tails = 0;
                for (int i = 0; i < 10000; i++) {
                    bool coinFlip = random.NextBool();
                    if (coinFlip) { heads++; } else { tails++; }
                }
                int diff = Math.Abs(heads - tails);
                // Assert.True(diff < 300, "Coin flips were not normally distributed around 0! diff=" + diff);
            }

            var randomName = random.NextRandomName();
            Log.d("The generated random name is: " + randomName);
            for (int i = 0; i < 1000; i++) { random.NextRandomName(); }

            // random.NextDouble() with a range from lowerBound to upperBound:
            double randomDouble = random.NextDouble(lowerBound: -100, upperBound: 100);
            Assert.InRange(randomDouble, -100, 100);

            // random.NextFloat() with a range from lowerBound to upperBound:
            float randomFloat = random.NextFloat(lowerBound: 20, upperBound: 50);
            Assert.InRange(randomFloat, 20, 50);

        }

        [Fact]
        public void RandomExtensions_MoreTests() {

            var random = new Random();
            Assert.False(random.NextFloat() == random.NextFloat() && random.NextFloat() == random.NextFloat());

            { // Test with MinValue and MaxValue:
                float f = random.NextFloat(float.MinValue, float.MaxValue);
                Assert.True(float.MinValue < f && f < float.MaxValue, "f=" + f);
            }

            { // Test with MinValue and MaxValue:
                double d = random.NextDouble(double.MinValue, double.MaxValue);
                Assert.True(double.MinValue < d && d < double.MaxValue, "d=" + d);
            }

            // Example for random.NextFloat():
            TestRandomFloat(random, 0, 1);
            TestRandomFloat(random, -0.1f, 0);
            TestRandomFloat(random, 10, 20000);
            TestRandomFloat(random, -100000, -100);
            TestRandomFloat(random, -10000, 10000);
            TestRandomFloat(random, float.MinValue, float.MaxValue);

            { // Example for random.NextDouble(min, max):
                double min = -1000;
                double max = 1000;
                double sum = 0;
                for (int i = 0; i < 10000; i++) {
                    double x = random.NextDouble(min, max);
                    Assert.InRange(x, min, max);
                    sum += x;
                } // The sum should be normally distributed around 0:
                Assert.InRange(sum, min * 200, max * 200);
            }

            { // Example for random.NextFloat(min, max):
                float min = -1000;
                float max = 1000;
                float sum = 0;
                for (int i = 0; i < 10000; i++) {
                    float x = random.NextFloat(min, max);
                    Assert.InRange(x, min, max);
                    sum += x;
                } // The sum should be normally distributed around 0:
                Assert.InRange(sum, min * 200, max * 200);
            }
        }

        private static void TestRandomFloat(Random random, float lowerBound, float upperBound) {
            double min = float.MaxValue;
            double max = float.MinValue;
            var results = new List<float>();
            for (int i = 0; i < 100000; i++) {
                float x = random.NextFloat(lowerBound, upperBound);
                Assert.True(x <= upperBound, "x=" + x);
                Assert.True(lowerBound <= x, "x=" + x);
                if (x < min) { min = x; }
                if (x > max) { max = x; }
                results.Add(x);
            }
            Assert.True(IsUniformlyDistributed(results));
            var reachedUpperBound = 100d * (1d - (upperBound - max) / (upperBound - lowerBound));
            var reachedLowerBound = 100d * (1d - (min - lowerBound) / (upperBound - lowerBound));
            Assert.True(reachedLowerBound > 98 && reachedUpperBound > 98, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
            Assert.True(reachedLowerBound <= 100 && reachedUpperBound <= 100, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
        }

        private static bool IsUniformlyDistributed(List<float> results) {
            return true; // TODO
        }

    }

}