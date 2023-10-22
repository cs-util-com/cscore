using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using com.csutil.algorithms;

namespace com.csutil.math {

    public static class CurveFitting {

        public static PolynomialFunction CalcAllPolynomialFunctionsWithRansac(IReadOnlyList<Vector2> points, Random rnd, double inlierDistance, float percent = 0.2f, int iterations = 1000, int maxCoefficientCount = 21) {
            int fractionOfTotalCount = (int)(points.Count * percent);
            return rnd.RunRansac(e: points, d: fractionOfTotalCount, minSampleSize: fractionOfTotalCount, iterations,
                createModel: (maybeInliers) => {
                    var functions = CalcAllPolynomialFunctions(maybeInliers.ToList(), maxCoefficientCount);
                    return functions.First();
                },
                isInlier: (model, elem) => {
                    var y = model.CalcPolynomialFor(elem.X);
                    return Math.Abs(y - elem.Y) < inlierDistance;
                });
        }

        /// <summary> Returns a list of polynomial functions that fit the given points, sorted by the median error so that the first entry in the
        /// list is typically the best one to use. </summary>
        public static IOrderedEnumerable<PolynomialFunction> CalcAllPolynomialFunctions(IReadOnlyList<Vector2> points, int maxCoefficientCount = 21) {
            var allFormulas = new List<PolynomialFunction>();
            for (int coefficientCount = 1; coefficientCount < maxCoefficientCount; coefficientCount++) {
                var res = new PolynomialFunction(CalcPolynomialCoefficients(points, coefficientCount));
                // Check that for each point the formula returns the correct y value:
                foreach (var p in points) {
                    var y = CalcPolynomialFor(res.Coefficients, p.X);
                    res.AllErrors.Add(Math.Abs(y - p.Y));
                }
                allFormulas.Add(res);
            }

            // If some of the formulas have the same median error, move the ones with the lowest number of coefficients to the front:
            var groupedByMedianError = allFormulas.GroupBy(x => x.ErrorMedian);
            var result = new List<PolynomialFunction>();
            foreach (var g in groupedByMedianError) {
                result.Add(g.OrderBy(x => x.CoefficientCount).First());
            }
            return result.OrderBy(x => x.ErrorMedian);
        }

        public class PolynomialFunction : Ransac.IModel<Vector2> {
            public double[] Coefficients { get; }

            public PolynomialFunction(double[] coefficients) { Coefficients = coefficients; }

            public List<double> AllErrors { get; } = new List<double>();
            public int CoefficientCount => Coefficients.Length;
            public double ErrorSum => AllErrors.Sum();
            public double ErrorMedian => AllErrors.CalcMedian();
            public double ErrorAverage => AllErrors.Average();

            public override string ToString() { return GetPolynomialStringFor(Coefficients); }

            public double CalcPolynomialFor(double x) { return CurveFitting.CalcPolynomialFor(Coefficients, x); }

            public double? totalModelError => ErrorMedian;
            public ICollection<Vector2> inliers { get; set; }
            public ICollection<Vector2> outliers { get; set; }

        }

        /// <summary> Calculates and returns the coefficients (a,b,c,d,e,..) of the
        /// polynomial y = a + b*x + c*x^2 + d*x^3 + e*x^4 + .. </summary>
        /// <param name="coefficientCount"> 2 for a line, 3 for a parabola, 4 for a cubic curve, etc..
        /// A good value to start testing is 3-5 to test if the curve fits your data well enough. </param>
        /// <returns> The coefficients of the polynomial </returns>
        public static double[] CalcPolynomialCoefficients(IReadOnlyList<Vector2> points, int coefficientCount = 5) {
            if (coefficientCount < 1) {
                throw new ArgumentException("Coefficient count must be at least 2 (which is a line).");
            }
            if (points.Count < coefficientCount) {
                // With fewer points than coefficients to determine, you have an under-determined system.
                // Reduce the coefficient count to the number of points:
                coefficientCount = points.Count;
            }
            int n = points.Count;
            // We will build the normal equations of the form: A^T * A * x = A^T * B
            // Where x are our coefficients
            double[,] A = new double[n, coefficientCount];
            double[] B = new double[n];

            for (int i = 0; i < n; i++) {
                double x = points[i].X;
                double y = points[i].Y;

                for (int j = 0; j < coefficientCount; j++) {
                    A[i, j] = Math.Pow(x, j);
                }
                B[i] = y;
            }

            double[,] ATA = new double[coefficientCount, coefficientCount];
            double[] ATB = new double[coefficientCount];

            // Compute A^T * A and A^T * B
            for (int i = 0; i < coefficientCount; i++) {
                for (int j = 0; j < coefficientCount; j++) {
                    double sum = 0;
                    for (int k = 0; k < n; k++) {
                        sum += A[k, i] * A[k, j];
                    }
                    ATA[i, j] = sum;
                }

                double sumB = 0;
                for (int k = 0; k < n; k++) {
                    sumB += A[k, i] * B[k];
                }
                ATB[i] = sumB;
            }

            // Solve for x using Gaussian elimination
            return GaussianElimination(ATA, ATB);
        }

        private static double[] GaussianElimination(double[,] A, double[] B) {
            int n = B.Length;
            for (int pivot = 0; pivot < n; pivot++) {
                int maxrow = pivot;
                for (int row = pivot + 1; row < n; row++) {
                    if (Math.Abs(A[row, pivot]) > Math.Abs(A[maxrow, pivot])) {
                        maxrow = row;
                    }
                }

                // Swap rows
                double[] temp = new double[n];
                for (int i = 0; i < n; i++) {
                    temp[i] = A[pivot, i];
                    A[pivot, i] = A[maxrow, i];
                    A[maxrow, i] = temp[i];
                }
                double t = B[pivot];
                B[pivot] = B[maxrow];
                B[maxrow] = t;

                // Pivot within A and B
                for (int row = pivot + 1; row < n; row++) {
                    double factor = A[row, pivot] / A[pivot, pivot];
                    B[row] -= factor * B[pivot];
                    for (int col = pivot; col < n; col++) {
                        A[row, col] -= factor * A[pivot, col];
                    }
                }
            }

            // Back substitution
            double[] solution = new double[n];
            for (int i = n - 1; i >= 0; i--) {
                double sum = B[i];
                for (int j = i + 1; j < n; j++) {
                    sum -= A[i, j] * solution[j];
                }
                solution[i] = sum / A[i, i];
            }
            return solution;
        }

        /// <summary> Calculates the y value for the given x value using the given polynomial coefficients </summary>
        public static double CalcPolynomialFor(double[] coefficients, double x) {
            var y = 0d;
            for (int i = 0; i < coefficients.Length; i++) {
                y += coefficients[i] * Math.Pow(x, i);
            }
            return y;
        }

        public static string GetPolynomialStringFor(double[] coefficients) {
            var s = "";
            for (int i = 0; i < coefficients.Length; i++) {
                double c = coefficients[i];
                if (c == 0) { continue; }
                if (s.Length > 0) { s += " + "; }
                if (c == 1d) {
                    s += ExpToString(i);
                } else {
                    s += CoefToString(c) + " * " + ExpToString(i);
                }
            }
            return "var y = " + s + ";";
        }

        private static string ExpToString(int exp) {
            if (exp == 0) { return "1"; }
            if (exp == 1) { return "x"; }
            return $"Math.Pow(x, {exp})";
        }

        private static string CoefToString(double d) { return d.ToString(new CultureInfo("en-US")); }

    }

}