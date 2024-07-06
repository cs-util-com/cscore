using System;
using StbImageSharp;

namespace com.csutil.algorithms.images {

    public struct Pixel {

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Pixel(byte r, byte g, byte b, byte a) {
            R = r;
            G = g;
            B = b;
            A = a;
        }

    }

    public static class ImageResultPixels {

        public static Pixel GetPixel(this ImageResult self, int x, int y) {
            int index = (y * self.Width + x) * (int)self.ColorComponents;
            if ((int)self.ColorComponents == 3) {
                return new Pixel(self.Data[index], self.Data[index + 1], self.Data[index + 2], 255);
            }
            if ((int)self.ColorComponents == 4) {
                return new Pixel(self.Data[index], self.Data[index + 1], self.Data[index + 2], self.Data[index + 3]);
            }
            throw new Exception("ColorComponents=" + self.ColorComponents);

        }

        public static void SetPixel(this ImageResult self, int x, int y, Pixel p) {
            int index = (y * self.Width + x) * (int)self.ColorComponents;
            if ((int)self.ColorComponents == 3) {
                self.Data[index] = p.R;
                self.Data[index + 1] = p.G;
                self.Data[index + 2] = p.B;
            } else if ((int)self.ColorComponents == 4) {
                self.Data[index] = p.R;
                self.Data[index + 1] = p.G;
                self.Data[index + 2] = p.B;
                self.Data[index + 3] = p.A;
            } else {
                throw new Exception("ColorComponents=" + self.ColorComponents);
            }
        }

    }

}