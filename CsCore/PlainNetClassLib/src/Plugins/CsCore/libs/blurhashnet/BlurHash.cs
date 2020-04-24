using System;
using System.Linq;
using System.Numerics;
using System.Text;

namespace blurhash {

    /// <summary> Represents a pixel within the Blurhash algorithm </summary>
    public struct Pixel { public double R, G, B; }

    /// <summary> Represents a 2D-coordinate </summary>
    public struct Coordinate { public int X, Y; }

    public static class Blurhash {


        /// <summary>
        /// The core encoding algorithm of Blurhash.
        /// To be not specific to any graphics manipulation library this algorithm only operates on <c>double</c> values.
        /// </summary>
        public static class Encoder {

            /// <summary>
            /// Encodes a 2-dimensional array of pixels into a Blurhash string
            /// </summary>
            /// <param name="pixels">The 2-dimensional array of pixels to encode</param>
            /// <param name="componentsX">The number of components used on the X-Axis for the DCT</param>
            /// <param name="componentsY">The number of components used on the Y-Axis for the DCT</param>
            /// <returns>The resulting Blurhash string</returns>
            public static string Encode(Pixel[,] pixels, int componentsX, int componentsY, Action<double> ProgressCallback = null) {
                if (componentsX < 1) { throw new ArgumentException("componentsX needs to be at least 1"); }
                if (componentsX > 9) { throw new ArgumentException("componentsX needs to be at most 9"); }
                if (componentsY < 1) { throw new ArgumentException("componentsY needs to be at least 1"); }
                if (componentsY > 9) { throw new ArgumentException("componentsY needs to be at most 9"); }

                var factors = new Pixel[componentsX, componentsY];

                var components = Enumerable.Range(0, componentsX)
                    .SelectMany(i => Enumerable.Range(0, componentsY).Select(j => new Coordinate() { X = i, Y = j }))
                    .ToArray(); // Create tuples (i,j) for all components

                var factorCount = componentsX * componentsY;
                var factorIndex = 0;

                for (int i = 0; i < components.Length; i++) {
                    var coordinate = components[i];
                    factors[coordinate.X, coordinate.Y] = MultiplyBasisFunction(coordinate.X, coordinate.Y, pixels);
                    ProgressCallback?.Invoke((double)factorIndex / factorCount);
                    factorIndex++;
                }

                var dc = factors[0, 0];
                var acCount = componentsX * componentsY - 1;
                var result = new StringBuilder();

                var sizeFlag = (componentsX - 1) + (componentsY - 1) * 9;
                result.Append(sizeFlag.EncodeBase83(1));

                float maxValue;
                if (acCount > 0) {
                    // Get maximum absolute value of all AC components
                    var actualMaxValue = 0.0;
                    for (var y = 0; y < componentsY; y++) {
                        for (var x = 0; x < componentsX; x++) {
                            // Ignore DC component
                            if (x == 0 && y == 0) continue;
                            actualMaxValue = Math.Max(Math.Abs(factors[x, y].R), actualMaxValue);
                            actualMaxValue = Math.Max(Math.Abs(factors[x, y].G), actualMaxValue);
                            actualMaxValue = Math.Max(Math.Abs(factors[x, y].B), actualMaxValue);
                        }
                    }

                    var quantizedMaxValue = (int)Math.Max(0.0, Math.Min(82.0, Math.Floor(actualMaxValue * 166 - 0.5)));
                    maxValue = ((float)quantizedMaxValue + 1) / 166;
                    result.Append(quantizedMaxValue.EncodeBase83(1));
                } else {
                    maxValue = 1;
                    result.Append(0.EncodeBase83(1));
                }

                result.Append(EncodeDc(dc.R, dc.G, dc.B).EncodeBase83(4));

                for (var y = 0; y < componentsY; y++) {
                    for (var x = 0; x < componentsX; x++) {
                        // Ignore DC component
                        if (x == 0 && y == 0) continue;
                        result.Append(EncodeAc(factors[x, y].R, factors[x, y].G, factors[x, y].B, maxValue).EncodeBase83(2));
                    }
                }
                return result.ToString();
            }

            private static Pixel MultiplyBasisFunction(int xComponent, int yComponent, Pixel[,] pixels) {
                double r = 0, g = 0, b = 0;
                double normalization = (xComponent == 0 && yComponent == 0) ? 1 : 2;

                var width = pixels.GetLength(0);
                var height = pixels.GetLength(1);

                for (var y = 0; y < height; y++) {
                    for (var x = 0; x < width; x++) {
                        var basis = Math.Cos(Math.PI * xComponent * x / width) * Math.Cos(Math.PI * yComponent * y / height);
                        r += basis * pixels[x, y].R;
                        g += basis * pixels[x, y].G;
                        b += basis * pixels[x, y].B;
                    }
                }

                var scale = normalization / (width * height);
                return new Pixel() { R = r * scale, G = g * scale, B = b * scale };
            }

            private static int EncodeAc(double r, double g, double b, double maxVal) {
                var quantizedR = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(r / maxVal, 0.5) * 9 + 9.5)));
                var quantizedG = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(g / maxVal, 0.5) * 9 + 9.5)));
                var quanzizedB = (int)Math.Max(0, Math.Min(18, Math.Floor(SignPow(b / maxVal, 0.5) * 9 + 9.5)));
                return quantizedR * 19 * 19 + quantizedG * 19 + quanzizedB;
            }

            private static int EncodeDc(double r, double g, double b) {
                var roundedR = LinearTosRgb(r);
                var roundedG = LinearTosRgb(g);
                var roundedB = LinearTosRgb(b);
                return (roundedR << 16) + (roundedG << 8) + roundedB;
            }

            /// <summary> Converts a linear double value into an sRGB input value (0 to 255) </summary>
            private static int LinearTosRgb(double value) {
                var v = Math.Max(0.0, Math.Min(1.0, value));
                if (v <= 0.0031308) return (int)(v * 12.92 * 255 + 0.5);
                else return (int)((1.055 * Math.Pow(v, 1 / 2.4) - 0.055) * 255 + 0.5);
            }

        }

        /// <summary>
        /// The core decoding algorithm of Blurhash.
        /// To be not specific to any graphics manipulation library this algorithm only operates on <c>double</c> values.
        /// </summary>
        public static class Decoder {

            /// <summary> Decodes a Blurhash string into a 2-dimensional array of pixels </summary>
            /// <param name="blurhash">The blurhash string to decode</param>
            /// <param name="outputWidth">The desired width of the output in pixels</param>
            /// <param name="outputHeight">The desired height of the output in pixels</param>
            /// <param name="punch">A value that affects the contrast of the decoded image. 1 means normal, smaller values will make the effect more subtle, and larger values will make it stronger.</param>
            /// <returns>A 2-dimensional array of <see cref="Pixel"/>s </returns>
            public static Pixel[,] Decode(string blurhash, int outputWidth, int outputHeight, double punch = 1.0, Action<double> ProgressCallback = null) {
                if (blurhash.Length < 6) {
                    throw new ArgumentException("Blurhash value needs to be at least 6 characters", nameof(blurhash));
                }

                var sizeFlag = (int)new[] { blurhash[0] }.DecodeBase83Integer();

                var componentsY = sizeFlag / 9 + 1;
                var componentsX = sizeFlag % 9 + 1;

                if (blurhash.Length != 4 + 2 * componentsX * componentsY) {
                    throw new ArgumentException("Blurhash value is missing data", nameof(blurhash));
                }

                var quantizedMaximumValue = (double)new[] { blurhash[1] }.DecodeBase83Integer();
                var maxValue = (quantizedMaximumValue + 1.0) / 166.0;

                var coefficients = new Pixel[componentsX, componentsY];
                {
                    var i = 0;
                    for (var y = 0; y < componentsY; y++) {
                        for (var x = 0; x < componentsX; x++) {
                            if (x == 0 && y == 0) {
                                var substring = blurhash.Substring(2, 4);
                                var value = substring.DecodeBase83Integer();
                                coefficients[x, y] = DecodeDc(value);
                            } else {
                                var substring = blurhash.Substring(4 + i * 2, 2);
                                var value = substring.DecodeBase83Integer();
                                coefficients[x, y] = DecodeAc(value, maxValue * punch);
                            }
                            i++;
                        }
                    }
                }

                var pixels = new Pixel[outputWidth, outputHeight];
                var pixelCount = outputHeight * outputWidth;
                var currentPixel = 0;

                var coordinates = Enumerable.Range(0, outputWidth)
                    .SelectMany(x => Enumerable.Range(0, outputHeight).Select(y => new Coordinate() { X = x, Y = y }))
                    .ToArray();

                for (int i = 0; i < coordinates.Length; i++) {
                    var coordinate = coordinates[i];
                    pixels[coordinate.X, coordinate.Y] = DecodePixel(componentsY, componentsX, coordinate.X, coordinate.Y, outputWidth, outputHeight, coefficients);
                    ProgressCallback?.Invoke((double)currentPixel / pixelCount);
                    currentPixel++;
                }

                return pixels;
            }

            private static Pixel DecodePixel(int componentsY, int componentsX, int x, int y, int width, int height, Pixel[,] coefficients) {
                var r = 0.0;
                var g = 0.0;
                var b = 0.0;
                for (var j = 0; j < componentsY; j++) {
                    for (var i = 0; i < componentsX; i++) {
                        var basis = Math.Cos((Math.PI * x * i) / width) * Math.Cos((Math.PI * y * j) / height);
                        var coefficient = coefficients[i, j];
                        r += coefficient.R * basis;
                        g += coefficient.G * basis;
                        b += coefficient.B * basis;
                    }
                }
                return new Pixel() { R = r, G = g, B = b };
            }

            private static Pixel DecodeDc(BigInteger value) {
                var intR = (int)value >> 16;
                var intG = (int)(value >> 8) & 255;
                var intB = (int)value & 255;
                return new Pixel() { R = SRgbToLinear(intR), G = SRgbToLinear(intG), B = SRgbToLinear(intB) };
            }

            /// <summary>
            /// Converts an sRGB input value (0 to 255) into a linear double value
            /// </summary>
            private static double SRgbToLinear(int value) {
                var v = value / 255.0;
                if (v <= 0.04045) return v / 12.92;
                else return Math.Pow((v + 0.055) / 1.055, 2.4);
            }

            private static Pixel DecodeAc(BigInteger value, double maximumValue) {
                var quantizedR = (double)(value / (19 * 19));
                var quantizedG = (double)((value / 19) % 19);
                var quantizedB = (double)(value % 19);
                return new Pixel() {
                    R = SignPow((quantizedR - 9.0) / 9.0, 2.0) * maximumValue,
                    G = SignPow((quantizedG - 9.0) / 9.0, 2.0) * maximumValue,
                    B = SignPow((quantizedB - 9.0) / 9.0, 2.0) * maximumValue
                };
            }

        }

        /// <summary> Calculates <c>Math.Pow(base, exponent)</c> but retains the sign of <c>base</c> in the result. </summary>
        /// <param name="base">The base of the power. The sign of this value will be the sign of the result</param>
        /// <param name="exponent">The exponent of the power</param>
        private static double SignPow(double @base, double exponent) {
            return Math.Sign(@base) * Math.Pow(Math.Abs(@base), exponent);
        }

    }

}