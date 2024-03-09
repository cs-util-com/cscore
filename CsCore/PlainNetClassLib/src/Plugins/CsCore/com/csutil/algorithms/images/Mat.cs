using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StbImageSharp;

namespace com.csutil.algorithms.images {
    public class Mat<T> {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Channels { get; private set; }
        public T[] data; // Contiguous memory block for pixel data

        public Mat(int width, int height, int channels) {
            Width = width;
            Height = height;
            Channels = channels;
            data = new T[width * height * channels]; // Allocate memory for pixels
        }
        public Mat(int width, int height, int channels, T[] data) : this(width, height, channels) {
            this.data = data;
        }

        public T this[int row, int col, int channel = 0] {
            get => data[(row * Width + col) * Channels + channel];
            set => data[(row * Width + col) * Channels + channel] = value;
        }
        public T[] GetPixel(int x, int y) {
            T[] pixel = new T[Channels];
            var index = (y * Width + x) * Channels;
            Array.Copy(data, index, pixel, 0, Channels);
            return pixel;
        }

        public void SetPixel(int x, int y, T[] pixel) {
            var index = (y * Width + x) * Channels;
            Array.Copy(pixel, 0, data, index, Channels);
        }

        // Access a pixel's channel value
        public T GetPixelChannel(int x, int y, int channel) {
            int index = (y * Width + x) * Channels + channel;
            return data[index];
        }

        // Set a pixel's channel value
        public void SetPixelChannel(int x, int y, int channel, T value) {
            int index = (y * Width + x) * Channels + channel;
            data[index] = value;
        }

        public void ColorEntireChannel(int channel, T color) {
            channel--; //more intuitive numeration of channel 1 to 4 instead of 0 to 3
            for (int i = channel; i < data.Length; i += Channels) {
                data[i] = color;
            }
        }
        public static Mat<float> ConvertByteToFloatMat(byte[] image, int width, int height, int colorchannels) {
            float[] imageRes = new float[image.Length];
            for (int i = 0; i < image.Length; i++) {
                imageRes[i] = (float)image[i];
            }
            return new Mat<float>(width, height, colorchannels, imageRes);
        }
        public static byte[] ConvertBackToByte(Mat<float> image) {
            byte[] result = new byte[image.data.Length];
            for (int i = 0; i < image.data.Length; i++) {
                result[i] = (byte)image.data[i];
            }
            return result;
        }

        private static Mat<T> Add(Mat<T> a, Mat<T> b) {
            if (a.Width != b.Width || a.Height != b.Height || a.Channels != b.Channels)
                throw new ArgumentException("Matrices dimensions or channels do not match.");

            Mat<T> result = new Mat<T>(a.Height, a.Width, a.Channels);

            Parallel.For(0, a.data.Length, i => {
                if (typeof(T) == typeof(int))
                    result.data[i] = (T)(object)(((int)(object)a.data[i]) + ((int)(object)b.data[i]));
                else if (typeof(T) == typeof(double))
                    result.data[i] = (T)(object)(((double)(object)a.data[i]) + ((double)(object)b.data[i]));
                else if (typeof(T) == typeof(byte))
                    result.data[i] = (T)(object)(byte)(((byte)(object)a.data[i]) + ((byte)(object)b.data[i]));
                else if (typeof(T) == typeof(float))
                    result.data[i] = (T)(object)(((float)(object)a.data[i]) + ((float)(object)b.data[i]));
            });

            return result;
        }

        private static Mat<T> Multiply(Mat<T> a, Mat<T> b) {
            if (a.Width != b.Height)
                throw new ArgumentException("Matrix A's width must match Matrix B's height.");

            Mat<T> result = new Mat<T>(a.Height, b.Width, 1);

            Parallel.For(0, result.Height, i => {
                for (int j = 0; j < result.Width; j++) {
                    if (typeof(T) == typeof(int)) {
                        int sum = 0;
                        for (int k = 0; k < a.Width; k++) {
                            result.data[i * result.Width + j] = (T)(object)(((int)(object)a.data[i * a.Width + k] * (int)(object)b.data[k * b.Width + j]));
                        }
                    }
                    if (typeof(T) == typeof(float)) {
                        float sum = 0;
                        for (int k = 0; k < a.Width; k++) {
                            result.data[i * result.Width + j] = (T)(object)(((float)(object)a.data[i * a.Width + k] * (float)(object)b.data[k * b.Width + j]));
                        }
                    }
                    if (typeof(T) == typeof(double)) {
                        double sum = 0;
                        for (int k = 0; k < a.Width; k++) {
                            result.data[i * result.Width + j] = (T)(object)(((double)(object)a.data[i * a.Width + k] * (double)(object)b.data[k * b.Width + j]));
                        }
                    }
                    if (typeof(T) == typeof(byte)) {
                        byte sum = 0;
                        for (int k = 0; k < a.Width; k++) {
                            result.data[i * result.Width + j] = (T)(object)(((byte)(object)a.data[i * a.Width + k] * (byte)(object)b.data[k * b.Width + j]));
                        }
                    }
                }
            });

            return result;
        }
        public string PrintMatrix() {
            var result = "";
            for (int i = 0; i < Height; i++) {
                for (int j = 0; j < Width; j++) {
                    // Access the element at [i, j] and print it, followed by a space
                    result += (data[i * Width + j] + " ");
                }
                // After printing all columns in the current row, print a newline character
                result += "\n";
            }
            return result;
        }

        public static Mat<T> operator +(Mat<T> a, Mat<T> b) {
            return Add(a, b);
        }
        public static Mat<T> operator *(Mat<T> a, Mat<T> b) { return Multiply(a, b); }

    }
}