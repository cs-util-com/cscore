using System;

namespace com.csutil {

    public static class RandomExtensions {

        public static double NextDouble(this Random random, double lowerBound, double upperBound) {
            return random.NextDouble() * (upperBound - lowerBound) + lowerBound;
        }

        public static float NextFloat(this Random random, float upperBound = 1) {
            // Source https://stackoverflow.com/a/3365388/165106 
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent) * upperBound;
        }

        public static float NextFloat(this Random random, float lowerBound, float upperBound) {
            return lowerBound + random.NextFloat() * (upperBound - lowerBound);
        }

        public static bool NextBool(this Random random) {
            // Source: https://stackoverflow.com/a/19191165/165106
            return random.NextDouble() > 0.5;
        }

    }

}