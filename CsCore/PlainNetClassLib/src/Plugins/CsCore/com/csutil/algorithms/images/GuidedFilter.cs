using System;
using System.Collections.Generic;
using System.Linq;

namespace com.csutil.algorithms.images {

    // Port of https://raw.githubusercontent.com/atilimcetin/guided-filter/master/guidedfilter.cpp 
    public class GuidedFilter {

        private readonly byte[] image;
        private readonly double[] imageDouble;
        private readonly int width;
        private readonly int height;
        private readonly int colorComponents;
        private readonly int radius;
        private readonly double eps;

        public static byte[] RunGuidedFilter(byte[] alpha, GuidedFilter instance) {
            return GuidedFilterImpl.Filter(alpha, instance.colorComponents, instance);
        }

        public GuidedFilter(byte[] image, int width, int height, int colorComponents, int radius, double eps) {
            this.image = image.DeepCopy();
            this.imageDouble = ConvertToDouble(image);
            this.width = width;
            this.height = height;
            this.colorComponents = colorComponents;
            this.radius = radius;
            this.eps = eps;
        }

        // Assumes images are store in RGB order 
        public GuidedFilter Init(int channels) {
            if (channels == 1) {
                return new GuidedFilterMono(image, width, height, colorComponents, 2 * radius + 1, eps);
            }
            if (channels == 4 || channels == 3) {
                return new GuidedFilterColor(image, width, height, colorComponents, 2 * radius + 1, eps);
            }
            throw new Exception("not correct channel image");
        }

        public static byte[] CombineRgb(byte[] singleChannel, int channel) {
            var result = new byte[singleChannel.Length * 3];
            var zeros = new byte[singleChannel.Length];
            if (channel == 0)
                result = CombineRGB(singleChannel, zeros, zeros, singleChannel.Length);
            else if (channel == 1)
                result = CombineRGB(zeros, singleChannel, zeros, singleChannel.Length);
            else
                result = CombineRGB(zeros, zeros, singleChannel, singleChannel.Length);
            return result;
        }

        private static byte[] CombineRGB(byte[] red, byte[] green, byte[] blue, int length) {
            var result = new byte[length * 3];
            for (int i = 0; i < length * 3; i++) {
                switch ((i % 3)) {
                    case 0:
                        result[i] = red[i / 3];
                        break;
                    case 1:
                        result[i] = green[i / 3];
                        break;
                    case 2:
                        result[i] = blue[i / 3];
                        break;
                    default:
                        result[i] = result[i];
                        break;
                }
            }
            return result;
        }

        public abstract class GuidedFilterImpl {
            /// <summary>
            /// Filters the image given to the GuidedFilter instance gF with the image given in p.
            /// Color components here has to be the number of channels image p has not the image of the GuidedFilter instance!
            /// </summary>
            /// <param name="p">Image with which the filter is guided</param>
            /// <param name="colorComponents">Color channel amount of image p, currently work for single or RGB and RGBA</param>
            /// <param name="gF">GuidedFilter instance with all needed information of the </param>
            /// <returns>The result of guiding the instanced image with image p</returns>
            public static byte[] Filter(in byte[] p, int colorComponents, GuidedFilter gF) {
                var t = Log.MethodEntered();
                var channel = FindFirstByte(p);
                var result = new byte[p.Length];
                if (colorComponents == 1) {
                    if (gF is GuidedFilterMono mono)
                        result = mono.FilterSingleChannel(p, channel);
                    else {

                        result = ConvertToByte(((GuidedFilterColor)gF).FilterSingleChannel(p));
                    }

                } else {
                    var pc = SplitChannels(p, colorComponents, gF);
                    var guidedChannels = new List<double[]>();
                    //only do this for RGB, but not for alpha channel
                    for (int i = 0; i < 3; ++i) {
                        guidedChannels.Add(((GuidedFilterColor)gF).FilterSingleChannel(pc[i]));
                    }
                    var orgImg = SplitChannels(gF.image, colorComponents, gF);
                    if (colorComponents == 3)
                        result = CombineRGB(guidedChannels[0], guidedChannels[1], guidedChannels[2], gF.width * gF.height);
                    else
                        result = CombineRGBA(guidedChannels[0], guidedChannels[1], guidedChannels[2], ConvertToDouble(orgImg[3]), gF.width * gF.height); //Only for 4 Channel Image!!!!
                }
                Log.MethodDone(t);
                return result;
            }
        }

        private class GuidedFilterMono : GuidedFilter {

            private readonly double[] meanI;
            private readonly double[] variance;

            public GuidedFilterMono(byte[] image, int width, int height, int colorComponents, int radius, double eps) :
                base(image, width, height, colorComponents, radius, eps) {

                meanI = BoxFilter(imageDouble, radius);
                var mean2 = BoxFilter(MultArrays(imageDouble, imageDouble), radius);
                variance = SubArrays(mean2, MultArrays(meanI, meanI));
            }


            public byte[] FilterSingleChannel(byte[] p, int currentChannel) {
                var pDouble = ConvertToDouble(p);
                var mean_p = BoxFilter(pDouble, radius);
                var mean_Ip = BoxFilter(MultArrays(imageDouble, pDouble), radius);
                var cov_Ip = SubArrays(mean_Ip, MultArrays(meanI, mean_p));

                var varianceEps = AddValueToSingleChannel(variance, eps, currentChannel);

                var a = DivideArrays(cov_Ip, varianceEps);
                var b = SubArrays(mean_p, MultArrays(a, meanI));

                var meanA = BoxFilter(a, radius);
                var meanB = BoxFilter(b, radius);

                return ConvertToByte(AddArrays(MultArrays(meanA, imageDouble), meanB));
            }
        }

        private class GuidedFilterColor : GuidedFilter {
            private double[] meanI_R, meanI_G, meanI_B;
            private readonly double[] redImageDouble, greenImageDouble, blueImageDouble;
            private readonly double[] invrr, invrg, invrb, invgg, invgb, invbb;

            public GuidedFilterColor(byte[] image, int width, int height, int colorComponents, int r, double eps) :
                base(image, width, height, colorComponents, r, eps) {

                var redImage = CreateSingleChannel(image, 0);
                var greenImage = CreateSingleChannel(image, 1);
                var blueImage = CreateSingleChannel(image, 2);
                redImageDouble = ConvertToDouble(redImage);
                greenImageDouble = ConvertToDouble(greenImage);
                blueImageDouble = ConvertToDouble(blueImage);

                meanI_R = BoxFilter(redImageDouble, r);
                meanI_G = BoxFilter(greenImageDouble, r);
                meanI_B = BoxFilter(blueImageDouble, r);


                // variance of I in each local patch: the matrix Sigma in Eqn (14).
                // Note the variance in each local patch is a 3x3 symmetric matrix:
                //           rr, rg, rb
                //   Sigma = rg, gg, gb
                //           rb, gb, bb
                var var_I_rr = BoxFilter(MultArrays(redImageDouble, redImageDouble), r)
                    .Zip(MultArrays(meanI_R, meanI_R), (x, y) => x - y + eps).ToArray();
                var var_I_rg = BoxFilter(MultArrays(redImageDouble, greenImageDouble), r)
                    .Zip(MultArrays(meanI_R, meanI_G), (x, y) => x - y).ToArray();
                var var_I_rb = BoxFilter(MultArrays(redImageDouble, blueImageDouble), r)
                    .Zip(MultArrays(meanI_R, meanI_B), (x, y) => x - y).ToArray();
                var var_I_gg = BoxFilter(MultArrays(greenImageDouble, greenImageDouble), r)
                    .Zip(MultArrays(meanI_G, meanI_G), (x, y) => x - y + eps).ToArray();
                var var_I_gb = BoxFilter(MultArrays(greenImageDouble, blueImageDouble), r)
                    .Zip(MultArrays(meanI_G, meanI_B), (x, y) => x - y).ToArray();
                var var_I_bb = BoxFilter(MultArrays(blueImageDouble, blueImageDouble), r)
                    .Zip(MultArrays(meanI_B, meanI_B), (x, y) => x - y + eps).ToArray();

                invrr = SubArrays(MultArrays(var_I_gg, var_I_bb), MultArrays(var_I_gb, var_I_gb));
                invrg = SubArrays(MultArrays(var_I_gb, var_I_rb), MultArrays(var_I_rg, var_I_bb));
                invrb = SubArrays(MultArrays(var_I_rg, var_I_gb), MultArrays(var_I_gg, var_I_rb));
                invgg = SubArrays(MultArrays(var_I_rr, var_I_bb), MultArrays(var_I_rb, var_I_rb));
                invgb = SubArrays(MultArrays(var_I_rb, var_I_rg), MultArrays(var_I_rr, var_I_gb));
                invbb = SubArrays(MultArrays(var_I_rr, var_I_gg), MultArrays(var_I_rg, var_I_rg));

                var covDet = AddArrays(AddArrays(MultArrays(invrr, var_I_rr),
                        MultArrays(invrg, var_I_rg)),
                    MultArrays(invrb, var_I_rb));

                invrr = DivideArrays(invrr, covDet);
                invrg = DivideArrays(invrg, covDet);
                invrb = DivideArrays(invrb, covDet);
                invgg = DivideArrays(invgg, covDet);
                invgb = DivideArrays(invgb, covDet);
                invbb = DivideArrays(invbb, covDet);
            }

            public double[] FilterSingleChannel(byte[] p) {
                var pDouble = ConvertToDouble(p);
                var meanP = BoxFilter(pDouble, radius);
                var meanIp_R = BoxFilter(MultArrays(redImageDouble, pDouble), radius);
                var meanIp_G = BoxFilter(MultArrays(greenImageDouble, pDouble), radius);
                var meanIp_B = BoxFilter(MultArrays(blueImageDouble, pDouble), radius);

                var cov_Ip_R = SubArrays(meanIp_R, MultArrays(meanI_R, meanP));
                var cov_Ip_G = SubArrays(meanIp_G, MultArrays(meanI_G, meanP));
                var cov_Ip_B = SubArrays(meanIp_B, MultArrays(meanI_B, meanP));

                var a_r = AddArrays(AddArrays(MultArrays(invrr, cov_Ip_R), MultArrays(invrg, cov_Ip_G)), MultArrays(invrb, cov_Ip_B));
                var a_g = AddArrays(AddArrays(MultArrays(invrg, cov_Ip_R), MultArrays(invgg, cov_Ip_G)), MultArrays(invgb, cov_Ip_B));
                var a_b = AddArrays(AddArrays(MultArrays(invrb, cov_Ip_R), MultArrays(invgb, cov_Ip_G)), MultArrays(invbb, cov_Ip_B));

                var b1 = SubArrays(meanP, MultArrays(a_r, meanI_R));
                var b2 = MultArrays(a_g, meanI_G);
                var b3 = MultArrays(a_b, meanI_B);
                var b = SubArrays(SubArrays(b1, b2), b3);

                var res1 = MultArrays(BoxFilter(a_r, radius), redImageDouble);
                var res2 = MultArrays(BoxFilter(a_g, radius), greenImageDouble);
                var res3 = MultArrays(BoxFilter(a_b, radius), blueImageDouble);
                var res4 = BoxFilter(b, radius);
                var result = AddArrays(AddArrays(AddArrays(res1, res2), res3), res4);
                return result;
            }
        }

        private double[] BoxFilter(double[] image, int radius) {
            return Filter.BoxFilterSingleChannel(image, width, height, radius / 2, 1);
        }

        private static byte[] ByteArrayAdd(byte[] image1, byte[] image2) {
            if (image1.Length != image2.Length) { throw new ArgumentException("Arrays must have the same length"); }
            var result = new byte[image1.Length];
            for (int i = 0; i < image1.Length; i++) {
                // Perform element-wise subtraction with underflow check
                var additionResult = image1[i] + image2[i];
                result[i] = (byte)Math.Min(Math.Max(additionResult, 0), 255);
            }
            return result;
        }

        private double[] AddValueToSingleChannel(double[] array, double eps, int channel) {
            var varianceEps = new double[array.Length];
            for (var i = 0; i < array.Length; i++) {
                if (channel == 3)
                    varianceEps[i] = array[i] + eps;
                else if (i % colorComponents == channel)
                    varianceEps[i] = array[i] + eps;
            }
            return varianceEps;
        }

        private static double[] MultArrays(double[] array1, double[] array2) {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }

            return array1.Zip(array2, (x, y) => x * y).ToArray();
        }

        private static double[] DivideArrays(double[] array1, double[] array2) {
            if (array1.Length != array2.Length) throw new ArgumentException("Input arrays must have the same length.");
            return array1.Zip(array2, (x, y) => (y != 0 && x != 0) ? x / y : x).ToArray();
        }

        private static double[] SubArrays(double[] array1, double[] array2) {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }
            return array1.Zip(array2, (x, y) => x - y).ToArray();
        }

        private static double[] AddArrays(double[] array1, double[] array2) {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }
            return array1.Zip(array2, (x, y) => x + y).ToArray();
        }

        public static double[] ConvertToDouble(byte[] data) {
            var res = new double[data.Length];
            for (var i = 0; i < data.Length; i++) {
                res[i] = data[i];
            }
            return res;
        }

        public static byte[] ConvertToByte(double[] data) {
            var res = new byte[data.Length];
            for (var i = 0; i < data.Length; i++) {
                res[i] = (byte)Math.Min(Math.Max(data[i], 0), 255);
            }
            return res;
        }
        private static byte[] CombineRGBA(double[] red, double[] green, double[] blue, double[] alpha, int length) {
            var result = new byte[length * 4];
            for (int i = 0; i < length * 4; i++) {
                switch (i % 4) {
                    case 0:
                        result[i] = (byte)Math.Min(Math.Max(red[i / 4], 0), 255);
                        break;
                    case 1:
                        result[i] = (byte)Math.Min(Math.Max(green[i / 4], 0), 255);
                        break;
                    case 2:
                        result[i] = (byte)Math.Min(Math.Max(blue[i / 4], 0), 255);
                        break;
                    case 3:
                        result[i] = (byte)Math.Min(Math.Max(alpha[i / 4], 0), 255);
                        break;
                }
            }
            return result;
        }

        private static byte[] CombineRGB(double[] red, double[] green, double[] blue, int length) {
            var result = new byte[length * 3];
            for (int i = 0; i < length * 3; i++) {
                switch (i % 3) {
                    case 0:
                        result[i] = (byte)Math.Min(Math.Max(red[i / 3], 0), 255);
                        break;
                    case 1:
                        result[i] = (byte)Math.Min(Math.Max(green[i / 3], 0), 255);
                        break;
                    case 2:
                        result[i] = (byte)Math.Min(Math.Max(blue[i / 3], 0), 255);
                        break;
                }
            }
            return result;
        }

        public byte[] CreateSingleChannel(byte[] image, int channel) {
            var newIm = new byte[image.Length / colorComponents];
            var count = 0;
            for (var i = 0; i < image.Length / colorComponents; i++) {
                newIm[i] = image[i * 4 + channel];
            }
            return newIm;
        }
        private static int FindFirstByte(byte[] image) {
            for (int i = 0; i <= 3; i++) {
                if (image[i] != 0) { return i; } //channel range from 0 to 3 in rgba order 
            }
            return -1;
        }

        private static List<byte[]> SplitChannels(byte[] inputImage, int numberOfChannels, GuidedFilter gf) {
            var result = new List<byte[]>();
            for (int i = 0; i < numberOfChannels; i++) {
                result.Add(gf.CreateSingleChannel(inputImage, i));
            }
            return result;
        }

    }

}