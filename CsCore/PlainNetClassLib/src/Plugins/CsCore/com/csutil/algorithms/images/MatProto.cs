using System;
using System.Collections.Generic;
using System.Text;

namespace com.csutil.algorithms.images {

    public class Mat {
        private double[,] data;

        public int Rows { get; private set; }
        public int Columns { get; private set; }

        public Mat(int rows, int columns) {
            Rows = rows;
            Columns = columns;
            data = new double[rows, columns];
        }

        public double this[int i, int j] {
            get { return data[i, j]; }
            set { data[i, j] = value; }
        }

        public static Mat operator +(Mat a, Mat b) {
            if (a.Rows != b.Rows || a.Columns != b.Columns) {
                throw new InvalidOperationException("Matrix dimensions must match");
            }
            var result = new Mat(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++) {
                for (int j = 0; j < a.Columns; j++) {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return result;
        }

        public static Mat operator -(Mat a, Mat b) {
            if (a.Rows != b.Rows || a.Columns != b.Columns) {
                throw new InvalidOperationException("Matrix dimensions must match");
            }
            var result = new Mat(a.Rows, a.Columns);
            for (int i = 0; i < a.Rows; i++) {
                for (int j = 0; j < a.Columns; j++) {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return result;
        }

        public static Mat operator *(Mat a, Mat b) {
            if (a.Columns != b.Rows) {
                throw new InvalidOperationException("The number of columns in the first matrix must equal the number of rows in the second matrix");
            }
            var result = new Mat(a.Rows, b.Columns);
            for (int i = 0; i < result.Rows; i++) {
                for (int j = 0; j < result.Columns; j++) {
                    for (int k = 0; k < a.Columns; k++) {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return result;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < Columns; j++) {
                    sb.Append(data[i, j] + "\t");
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
