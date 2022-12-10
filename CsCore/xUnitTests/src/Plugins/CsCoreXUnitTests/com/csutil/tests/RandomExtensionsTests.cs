using System;
using Xunit;
using com.csutil.random;
using System.Collections.Generic;
using System.Linq;

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
            var results = new List<double>();
            for (int i = 0; i < 100000; i++) {
                float x = random.NextFloat(lowerBound, upperBound);
                Assert.True(x <= upperBound, "x=" + x);
                Assert.True(lowerBound <= x, "x=" + x);
                if (x < min) { min = x; }
                if (x > max) { max = x; }
                results.Add(x);
            }
            AssertIsRandomDistribution(results);
            var reachedUpperBound = 100d * (1d - (upperBound - max) / (upperBound - lowerBound));
            var reachedLowerBound = 100d * (1d - (min - lowerBound) / (upperBound - lowerBound));
            Assert.True(reachedLowerBound > 98 && reachedUpperBound > 98, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
            Assert.True(reachedLowerBound <= 100 && reachedUpperBound <= 100, "min%=" + reachedLowerBound + ", max%=" + reachedUpperBound);
        }

        private static void AssertIsRandomDistribution(IEnumerable<double> results) {
            var minVal = results.Min();
            // Since RSD only works if mean not close to zero shift the value range into positive range if needed:
            if (minVal < 0) {
                minVal = Math.Abs(minVal);
                results = results.Map(x => (x + minVal));
            }
            var relStandardDeviation = results.GetRelativeStandardDeviation();
            Assert.True(0.57 < relStandardDeviation && relStandardDeviation < 0.59, "relStandardDeviation=" + relStandardDeviation + ", mean=" + results.Average());
        }

        [Fact]
        public void TestRelativeStandardDeviation() {
            {
                var numbers = new List<double>();
                for (int i = 0; i < 100000; i++) { numbers.Add(i); }
                AssertIsRandomDistribution(numbers);
            }
        }

        [Fact]
        public void TestNextRndChild() {
            var random = new Random();
            var list = new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" };
            var counters = new Dictionary<string, int>();
            for (int i = 0; i < 10000; i++) {
                var child = random.NextRndChild(list);
                counters[child] = counters.GetValueOrDefault(child, 0) + 1;
            }
            var relStandardDeviation = counters.Map(x => (double)x.Value).GetRelativeStandardDeviation();
            // True randomness should cause all counters to be roughly the same (so RSD should be close to 0): 
            Assert.True(relStandardDeviation < 0.05, "relStandardDeviation=" + relStandardDeviation);
        }

        [Fact]
        public void TestCollectionShuffle() {
            var random = new Random();
            var list = new string[] { "a", "b", "c", "d", "e" };
            var shuffeled = random.ShuffleEntries(list);
            for (int i = 0; i < 100; i++) {
                Assert.Equal(shuffeled.First(), shuffeled.First());
                Assert.Equal("a", list.First());
                Assert.Equal(shuffeled.Last(), shuffeled.Last());
                Assert.Equal("e", list.Last());
            }
            
            var selectedOnes = new HashSet<string>();
            for (int i = 0; i < 1000; i++) {
                selectedOnes.Add(random.SampleElemsToGetRandomSubset(list, 1).Single());
            }
            Assert.Equal(list, selectedOnes.OrderBy(x => x));
        }

    }

}