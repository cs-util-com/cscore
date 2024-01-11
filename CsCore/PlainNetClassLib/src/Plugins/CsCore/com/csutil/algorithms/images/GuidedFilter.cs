using System;
using System.IO;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
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
                var mean2 = BoxFilter(ByteArrayMult(image, image), r);
                variance = ByteArraySub(mean2, ByteArrayMult(mean1, mean1));
            }


            public byte[] GuidedFilterSingleChannel(byte[] imageSingleChannel, int currentChannel) {
                var numberImage = ConvertToDouble(imageSingleChannel);
                var mean_p = BoxFilter(imageSingleChannel, r);
                var mean_Ip = BoxFilter(ByteArrayMult(imageSingleChannel, mean_p), r);
                var cov_Ip = ByteArraySub(mean_Ip, ByteArrayMult(mean1, mean_p));

                var meanNumber = ConvertToDouble(mean_p);
                var covariance = ConvertToDouble(cov_Ip);
                var varianceNumber = ConvertToDouble(variance);
                var meanI = ConvertToDouble(mean1);
                
                var varianceEps = new double[variance.Length];
                for (var i = 0; i < variance.Length; i++) {
                    if(i % colorComponents == currentChannel)
                        varianceEps[i] = varianceNumber[i] + eps;
                }
                var a = DivideArrays(covariance, varianceEps);
                var b = SubArrays(meanNumber, MultArrays(a, meanI));

                var meanA = BoxFilter(ConvertToByte(a), r);
                var meanB = BoxFilter(ConvertToByte(b), r);

                return ByteArrayAdd(ByteArrayMult(meanA, image), meanB);
            }
        }

        public static byte[] RunGuidedFilter(byte[] bytes, byte[] alpha, int i, double eps) {
            throw new NotImplementedException("TODO");
        }


        private byte[] BoxFilter(byte[] image, int boxSize) {
            return ImageBlur.RunBoxBlur(image, width, height, boxSize, colorComponents);
        }


        
        
        public static byte[] ByteArrayMult(byte[] image1, byte[] image2) {
            if (image1.Length != image2.Length) throw new ArgumentException("Input arrays must have the same length.");
            var result = new byte[image1.Length];
            for (var i = 0; i < image1.Length; i++) {
                var intResult = (int)image1[i] * image2[i];
                result[i] = (byte)Math.Min(byte.MaxValue, Math.Max(0, intResult));
            }
            return result;
        }
        
        public static byte[] ByteArraySub(byte[] image1, byte[] image2) {
            if (image1.Length != image2.Length) { throw new ArgumentException("Arrays must have the same length"); }

            var length = image1.Length;
            var result = new byte[length];

            for (int i = 0; i < length; i++) {
                // Perform element-wise subtraction with underflow check
                var subtractionResult = image1[i] - image2[i];

                if (subtractionResult < 0) {
                    // Underflow occurred, set result to 0
                    result[i] = 0;
                } else {
                    result[i] = (byte)subtractionResult;
                }
            }

            return result;
        }
        
        public static byte[] ByteArrayAdd(byte[] image1, byte[] image2) {
            if (image1.Length != image2.Length) { throw new ArgumentException("Arrays must have the same length"); }

            var length = image1.Length;
            var result = new byte[length];

            for (int i = 0; i < length; i++) {
                // Perform element-wise subtraction with underflow check
                var additionResult = image1[i] + image2[i];

                if (additionResult > 255) {
                    // Underflow occurred, set result to 0
                    result[i] = 255;
                } else {
                    result[i] = (byte)additionResult;
                }
            }

            return result;
        }
        
        public static double[] MultArrays(double[] array1, double[] array2)
        {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }

            var result = new double[array1.Length];
            for (var i = 0; i < array1.Length; i++){
                result[i] = array1[i] * array2[i];
            }

            return result;
        }
        
        private static double[] DivideArrays(double[] array1, double[] array2) {
            if (array1.Length != array2.Length) throw new ArgumentException("Input arrays must have the same length.");
            var res = new double[array1.Length];
            for (var i = 0; i < array1.Length; i++) {
                res[i] = array1[i] / array2[i];
            }
            return res;
        }
        
        private static double[] SubArrays(double[] array1, double[] array2)
        {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }

            var result = new double[array1.Length];
            for (var i = 0; i < array1.Length; i++){
                result[i] = array1[i] - array2[i];
            }
            return result;
        }
        
        
        
        private static double[] AddArrays(double[] array1, double[] array2)
        {
            if (array1 == null || array2 == null) { throw new ArgumentException("Input arrays cannot be null."); }
            if (array1.Length != array2.Length) { throw new ArgumentException("Input arrays must have the same length."); }

            var result = new double[array1.Length];
            for (var i = 0; i < array1.Length; i++){
                result[i] = array1[i] - array2[i];
            }
            return result;
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
                res[i] = (byte)data[i];
            }
            return res;
        }


        public  byte[] CreateSingleChannel(byte[] image, int channel) {
            var newIm = new byte[image.Length];
            for (var i = 0; i < image.Length; i++) {
                if (i % colorComponents == channel) {
                    newIm[i] = image[i];
                } else if(i % colorComponents == 3 && colorComponents == 4){
                    newIm[i] = 255;
                } else {
                    newIm[i] = 0;
                }
            }
            return newIm;
        }
    }
}