﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace com.csutil.math {

    public static class CalculateAverage {

        public static float CalcMean(this IEnumerable<float> self) {
            return (float)self.CalcMean(x => x);
        }

        public static double CalcMean(this IEnumerable<double> self) {
            return self.CalcMean(x => x);
        }

        public static double CalcMean<T>(this IEnumerable<T> self, Func<T, double> selector) {
            if (self.IsEmpty()) { return double.NaN; }
            return self.Average(selector);
        }

        public static Vector3 CalcMean(this IEnumerable<Vector3> sequence) {
            sequence = sequence.Cached();
            var x = sequence.CalcMean(v => v.X);
            var y = sequence.CalcMean(v => v.Y);
            var z = sequence.CalcMean(v => v.Z);
            return new Vector3((float)x, (float)y, (float)z);
        }
        
        public static Vector3 CalcRunningMean(Vector3 oldAverage, Vector3 newValue, int count) {
            var newX = CalcRunningMean(oldAverage.X, newValue.X, count);
            var newY = CalcRunningMean(oldAverage.Y, newValue.Y, count);
            var newZ = CalcRunningMean(oldAverage.Z, newValue.Z, count);
            return new Vector3(newX, newY, newZ);
        }
        
        /// <summary> Internally uses Quaternion.Slerp to approximate the latest mean rotation </summary>
        /// <param name="oldAverage"> The current average </param>
        /// <param name="newValue"> The new value that should contribute to the old average </param>
        /// <param name="count"> The number of elements that already contributed to the average </param>
        public static Quaternion CalcRunningMean(this Quaternion oldAverage, Quaternion newValue, int count) {
            return Quaternion.Slerp(oldAverage, newValue, 1f / count);
        }

        /// <summary>
        /// This allows to calculate the mean of a list without storing all entries of the list.
        /// See https://math.stackexchange.com/questions/106700/incremental-averageing
        /// </summary>
        /// <param name="oldAverage">the old mean value</param>
        /// <param name="newValue">the new value added to the mean</param>
        /// <param name="count">the total count of entries considered for the mean</param>
        /// <returns>the new average</returns>
        public static float CalcRunningMean(float oldAverage, float newValue, int count) {
            return (oldAverage * count + newValue) / (count + 1f);
        }

        /// <summary>
        /// Fourth order zero-phase shift low-pass Butterworth filter function by Sam Van Wassenbergh (University of Antwerp, 2007)
        /// Source: https://www.codeproject.com/Tips/1092012/A-Butterworth-Filter-in-Csharp
        /// </summary>
        /// <param name="self">The unfiltered data</param>
        /// <param name="dtInSec">the time between one data point and the next in seconds, is needed to calculate the sampling rate (inverse of the time step)</param>
        /// <param name="CutOff">desired cutoff frequency in Hz</param>
        /// <returns></returns>
        public static double[] CalcButterworthAvg(this double[] self, double dtInSec, double CutOff) {
            if (self == null) { return null; }
            if (CutOff == 0) { return self; }

            double Samplingrate = 1 / dtInSec;
            long dF2 = self.Length - 1; // The data range is set with dF2
            double[] Dat2 = new double[dF2 + 4]; // Array with 4 extra points front and back
            double[] data = self; // Ptr., changes passed data

            // Copy indata to Dat2
            for (long r = 0; r < dF2; r++) { Dat2[2 + r] = self[r]; }
            Dat2[1] = Dat2[0] = self[0];
            Dat2[dF2 + 3] = Dat2[dF2 + 2] = self[dF2];
            double wc = Math.Tan(CutOff * Math.PI / Samplingrate);
            double k1 = 1.414213562 * wc; // Sqrt(2) * wc
            double k2 = wc * wc;
            double a = k2 / (1 + k1 + k2);
            double b = 2 * a;
            double c = a;
            double k3 = b / k2;
            double d = -2 * a + k3;
            double e = 1 - (2 * a) - k3;

            // RECURSIVE TRIGGERS - ENABLE filter is performed (first, last points constant)
            double[] DatYt = new double[dF2 + 4];
            DatYt[1] = DatYt[0] = self[0];
            for (long s = 2; s < dF2 + 2; s++) {
                DatYt[s] = a * Dat2[s] + b * Dat2[s - 1] + c * Dat2[s - 2] + d * DatYt[s - 1] + e * DatYt[s - 2];
            }
            DatYt[dF2 + 3] = DatYt[dF2 + 2] = DatYt[dF2 + 1];

            // FORWARD filter
            double[] DatZt = new double[dF2 + 2];
            DatZt[dF2] = DatYt[dF2 + 2];
            DatZt[dF2 + 1] = DatYt[dF2 + 3];
            for (long t = -dF2 + 1; t <= 0; t++) {
                DatZt[-t] = a * DatYt[-t + 2] + b * DatYt[-t + 3] + c * DatYt[-t + 4] + d * DatZt[-t + 1] + e * DatZt[-t + 2];
            }

            // Calculated points copied for return
            for (long p = 0; p < dF2; p++) { data[p] = DatZt[p]; }
            return data;
        }

    }

}