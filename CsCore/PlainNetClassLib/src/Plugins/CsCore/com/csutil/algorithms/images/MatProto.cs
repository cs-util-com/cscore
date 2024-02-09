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

        public static Mat ConvertImageToMat(byte[] byteArray, int width, int height, int channels) {
            if (byteArray.Length != width * height * channels)
                throw new ArgumentException("The size of the byte array does not match the specified dimensions and channels.");

            // Adjusting the matrix size to accommodate the channel data
            // Each "pixel" will occupy 'channels' consecutive elements in a row
            Mat mat = new Mat(height, width * channels);
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width; j++) {
                    for (int ch = 0; ch < channels; ch++) {
                        // Calculating the index for the byte array and the column index in the matrix
                        int byteArrayIndex = (i * width + j) * channels + ch;
                        int matColIndex = j * channels + ch;

                        // Assuming each byte directly maps to a double value in the matrix
                        mat[i, matColIndex] = byteArray[byteArrayIndex];
                    }
                }
            }
            return mat;
        }
        public byte[] ToByteArray(int channels) {
            if (Cols % channels != 0)
                throw new ArgumentException("The number of columns in the matrix is not divisible by the specified number of channels, indicating a mismatch.");

            int width = Cols / channels;
            byte[] byteArray = new byte[Rows * width * channels];

            for (int i = 0; i < Rows; i++) {
                for (int j = 0; j < width; j++) {
                    for (int ch = 0; ch < channels; ch++) {
                        int byteArrayIndex = (i * width + j) * channels + ch;
                        int matColIndex = j * channels + ch;

                        // Assuming the data in Mat can be directly mapped back to a byte
                        // This might require scaling or conversion depending on the data range
                        byteArray[byteArrayIndex] = (byte)data[i, matColIndex];
                    }
                }
            }
            return byteArray;
        }
        public static Mat Copy(Mat sourceMat) {
            Mat newMat = new Mat(sourceMat.Rows, sourceMat.Cols);
            for (int i = 0; i < sourceMat.Rows; i++) {
                for (int j = 0; j < sourceMat.Cols; j++) {
                    newMat.data[i, j] = sourceMat.data[i, j];
                }
            }
            return newMat;
        }


        // Method to set color values at a given position (x, y) considering channels
        public void SetColor(int x, int y, int channels, double[] colorValues) {
            if (colorValues.Length != channels)
                throw new ArgumentException("Color values array length must match the number of channels.");

            for (int ch = 0; ch < channels; ch++) {
                if (y * channels + ch < Cols)
                    data[x, y * channels + ch] = colorValues[ch];
                //else
                    //throw new ArgumentOutOfRangeException("Position out of bounds.");
            }
        }


        // Method to get color values at a given position (x, y) considering channels
        public double[] GetColor(int x, int y, int channels) {
            double[] colorValues = new double[channels];

            for (int ch = 0; ch < channels; ch++) {
                if (y * channels + ch < Cols && x < Rows)
                    colorValues[ch] = data[x, y * channels + ch];
               // else
                    //throw new ArgumentOutOfRangeException("Position out of bounds." + "Number of Cols: " + Cols + "  Number of Rows: " + Rows + "  Col to be accessed: " + y  +  "  Row to be accessed: " + x);
            }

            return colorValues;
        }

    }
}
