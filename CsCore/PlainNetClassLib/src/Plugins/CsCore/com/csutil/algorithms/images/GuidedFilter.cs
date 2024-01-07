using System;
using com.csutil.http.apis;

namespace com.csutil.algorithms.images {

    // TODO create a port of https://raw.githubusercontent.com/atilimcetin/guided-filter/master/guidedfilter.cpp 
    public class GuidedFilter {

        private byte[] image;
        private int width;
        private int height;
        private int colorComponents;
        private int r;
        private double eps;

        public GuidedFilter(byte[] image, int width, int height, int colorComponents, int r, double eps) {
            this.image = image;
            this.width = width;
            this.height = height;
            this.colorComponents = colorComponents;
            this.r = r;
            this.eps = eps;
        }

        public class GuidedFilterMono : GuidedFilter {

            private byte[] channel;
            public byte[] mean1;
            public byte[] variance;
            public GuidedFilterMono(byte[] image, int width, int height, int colorComponents, int r, double eps) :
                base(image, width, height, colorComponents, r, eps) {

                mean1 = BoxFilter(image, r);
                var mean2 = BoxFilter((ElementwiseMultiply(image, image)), r);
                variance = ElementwiseSub(mean2, ElementwiseMultiply(mean1, mean1));
            }


            public byte[] GuidedFilterSingleChannel(byte[] imageSingleChannel) {
                var mean_p = BoxFilter(imageSingleChannel, r);
                var mean_Ip = BoxFilter(ElementwiseMultiply(imageSingleChannel, mean_p), r);
                var cov_Ip = ElementwiseSub(mean_Ip, ElementwiseMultiply(mean1, mean_p));

                var varianceEps = new double[variance.Length];
                for (int i = 0; i < variance.Length; i++) {
                    varianceEps[i] = variance[i] + eps;
                }
                //var a = cov_Ip / (variance + eps)


                return imageSingleChannel;
            }
        }

        public static byte[] RunGuidedFilter(byte[] bytes, byte[] alpha, int i, double eps) {
            throw new NotImplementedException("TODO");
        }


        private byte[] BoxFilter(byte[] image, int boxSize) {
            return ImageBlur.RunBoxBlur(image, width, height, boxSize, colorComponents);
        }


        public static byte[] ElementwiseMultiply(byte[] image1, byte[] image2) {
            if (image1.Length != image2.Length)
                throw new ArgumentException("Input arrays must have the same length.");

            byte[] result = new byte[image1.Length];

            for (int i = 0; i < image1.Length; i++) {
                // Convert bytes to integers for intermediate calculations
                int intResult = (int)image1[i] * image2[i];

                // Check for overflow and clip the result to byte range
                result[i] = (byte)Math.Min(byte.MaxValue, Math.Max(0, intResult));
            }

            return result;
        }
        
        
        public static byte[] ElementwiseSub(byte[] image1, byte[] image2)
        {
            if (image1.Length != image2.Length)
            {
                throw new ArgumentException("Arrays must have the same length");
            }

            int length = image1.Length;
            byte[] result = new byte[length];

            for (int i = 0; i < length; i++)
            {
                // Perform element-wise subtraction with underflow check
                int subtractionResult = image1[i] - image2[i];

                if (subtractionResult < 0)
                {
                    // Underflow occurred, set result to 0
                    result[i] = 0;
                }
                else
                {
                    result[i] = (byte)subtractionResult;
                }
            }

            return result;
        }

        private byte[] CreateSingleChannel(byte[] image, int channel) {
            var newIm = new byte[image.Length];
            switch (channel) {
                case 0:
                    for (var i = 0; i < image.Length; i++) {
                        if (i%width == 0) {
                            newIm[i] = image[i];
                        } else {
                            newIm[i] = 0;
                        }
                    }
                    break;
                case 1:
                    for (var i = 0; i < image.Length; i++) {
                        if (i%width == 1) {
                            newIm[i] = image[i];
                        } else {
                            newIm[i] = 0;
                        }
                    }
                    break;
                case 2:
                    for (var i = 0; i < image.Length; i++) {
                        if (i%width == 2) {
                            newIm[i] = image[i];
                        } else {
                            newIm[i] = 0;
                        }
                    }
                    break;
                default:
                    newIm = image;
                    break;
            }

            return newIm;
        }
    }
}