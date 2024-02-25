using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using StbImageSharp;

namespace com.csutil.algorithms.images
{
    public class Mat
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Channels { get; private set; }
        public double[] data; // Contiguous memory block for pixel data

        public Mat(int width, int height, int channels)
        {
            Width = width;
            Height = height;
            Channels = channels;
            data = new double[width * height * channels]; // Allocate memory for pixels
        }
        public Mat(int width, int height, int channels, double[] data) : this(width, height, channels)
        {
            this.data = data;
        }

        public double this[int row, int col, int channel = 0]
        {
            get => data[(row * Width + col) * Channels + channel];
            set => data[(row * Width + col) * Channels + channel] = value;
        }
        public double[] GetPixel(int x, int y)
        {
            double[] pixel = new double[Channels];
            var index = (y * Width + x) * Channels;
            Array.Copy(data, index, pixel, 0, Channels);
            return pixel;
        }

        public void SetPixel(int x, int y, double[] pixel)
        {
            var index = (y * Width + x) * Channels;
            Array.Copy(pixel, 0, data, index, Channels);
        }

        // Access a pixel's channel value
        public double GetPixelChannel(int x, int y, int channel)
        {
            int index = (y * Width + x) * Channels + channel;
            return data[index];
        }

        // Set a pixel's channel value
        public void SetPixelChannel(int x, int y, int channel, double value)
        {
            int index = (y * Width + x) * Channels + channel;
            data[index] = value;
        }
        
        public void ColorEntireChannel(int channel, int color)
        {
            channel--; //more intuitive numeration of channel 1 to 4 instead of 0 to 3
            for(int i = channel; i < data.Length; i += Channels)
            {
                data[i] = color;
            }
        }
        public static Mat ConvertByteToMat(byte[] image, int width, int height, int colorchannels)
        {
            double[] imageRes = new double[image.Length];
            for (int i = 0; i < image.Length; i++)
            {
                imageRes[i] = (double)image[i];
            }
            return new Mat(width, height, colorchannels, imageRes);
        }
        public static byte[] ConvertBackToByte(Mat image)
        {
            byte[] result = new byte[image.data.Length];
            for(int i = 0; i < image.data.Length; i++)
            {
                result[i] = (byte)image.data[i];
            }
            return result;
        }
    }
}
