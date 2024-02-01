using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace com.csutil.algorithms.images {

    public class Mat {
        public int Rows { get; private set; }
        public int Cols { get; private set; }
        public double[,] data;

        public Mat(int rows, int cols) {
            Rows = rows;
            Cols = cols;
            data = new double[rows, cols];
        }

        public double this[int row, int col] {
            get { return data[row, col]; }
            set { data[row, col] = value; }
        }

        // Operator overloading for addition
        public static Mat operator +(Mat a, Mat b) => Operate(a, b, (x, y) => x + y);

        // Operator overloading for subtraction
        public static Mat operator -(Mat a, Mat b) => Operate(a, b, (x, y) => x - y);

        // Method for matrix multiplication
        public static Mat operator *(Mat a, Mat b) {
            if (a.Cols != b.Rows)
                throw new ArgumentException("Matrices dimensions do not match for multiplication.");

            Mat result = new Mat(a.Rows, b.Cols);
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Cols; j++) {
                    double sum = 0;
                    for (int k = 0; k < a.Cols; k++) {
                        sum += a[i, k] * b[k, j];
                    }
                    result[i, j] = sum;
                }
            }
            return result;
        }

        // Element-wise multiplication 
        public static Mat ElementWiseMultiply(Mat a, Mat b) {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("Matrices dimensions do not match for element-wise multiplication.");

            Mat result = new Mat(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++) {
                for (int j = 0; j < a.Cols; j++) {
                    result[i, j] = a[i, j] * b[i, j];
                }
            }
            return result;
        }

        private static Mat Operate(Mat a, Mat b, Func<double, double, double> operation) {
            if (a.Rows != b.Rows || a.Cols != b.Cols)
                throw new ArgumentException("Matrices dimensions do not match.");

            Mat result = new Mat(a.Rows, a.Cols);
            for (int i = 0; i < a.Rows; i++) {
                for (int j = 0; j < a.Cols; j++) {
                    result[i, j] = operation(a[i, j], b[i, j]);
                }
            }
            return result;
        }

        public void Display() {
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Cols; j++) {
                    Console.Write($"{data[i, j],8:F2} ");
                }
                Console.WriteLine();
            }
        }
    }
}
