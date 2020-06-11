using System;
using System.Collections.Generic;

namespace com.csutil {

    public static class RandomExtensions {

        public static double NextDouble(this Random random, double lowerBound, double upperBound) {
            double resolutionFactor = 2; // To avoid problems with max and min double values
            upperBound = upperBound / resolutionFactor;
            lowerBound = lowerBound / resolutionFactor;
            double f = (upperBound - lowerBound) * random.NextDouble() + lowerBound;
            return f * resolutionFactor;
        }

        public static float NextFloat(this Random random, float lowerBound = 0, float upperBound = 1) {
            return (float)(NextDouble(random, lowerBound, upperBound));
        }

        // Returns a value between float.MinValue/2 and float.MaxValue/2
        private static float GetRndValueInHalfFloatRange(Random random) {
            // Source https://stackoverflow.com/a/3365388/165106 
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }

        public static bool NextBool(this Random random) {
            // Source: https://stackoverflow.com/a/19191165/165106
            return random.NextDouble() > 0.5;
        }

    }

}

namespace com.csutil.random {

    public static class RandomNameGenerator {
        // Not really core but it was fun to build ;) And might help with generating random data for testing

        // v for vowels 1 for the first set of consonants and 2 for the second set of consonants:
        public static List<string> generatorInstructions = new List<string>() { "v2", "v2v", "1v2", "v2v2", "1v2v2" };
        public static List<string> vowels = new List<string>() { "a", "e", "i", "o", "u", "ei", "ai", "ou", "j", "ji", "y", "oi", "au", "oo" };
        public static List<string> consonants1 = new List<string>() { "b", "c", "d", "f", "g", "h", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "x", "z", "ch", "bl", "br", "fl", "gl", "gr", "kl", "pr", "st", "sh", "th" };
        public static List<string> consonants2 = new List<string>() { "b", "d", "f", "g", "h", "k", "l", "m", "n", "p", "r", "s", "t", "v", "w", "z", "ch", "gh", "nn", "st", "sh", "th", "tt", "ss", "pf", "nt" };

        public static String NextRandomName(this Random self) {
            string instructionSet = self.NextRandomListEntry(generatorInstructions);
            return NextRandomName(self, instructionSet, vowels, consonants1, consonants2);
        }

        public static string NextRandomName(this Random self, string genertorInstruction, List<string> vowels, List<string> consonants1, List<string> consonants2) {
            String generatedName = "";
            int length = genertorInstruction.Length;
            for (int i = 0; i < length; i++) {
                char c = genertorInstruction[0];
                switch (c) {
                    case 'v': generatedName += self.NextRandomListEntry(vowels); break;
                    case '1': generatedName += self.NextRandomListEntry(consonants1); break;
                    case '2': generatedName += self.NextRandomListEntry(consonants2); break;
                }
                genertorInstruction = genertorInstruction.Substring(1);
            }
            return generatedName.ToFirstCharUpperCase();
        }

        private static T NextRandomListEntry<T>(this Random self, List<T> list) { return list[self.Next(0, list.Count - 1)]; }

    }
}