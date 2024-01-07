using System;

namespace com.csutil.algorithms.images {
    
    // TODO create a port of https://raw.githubusercontent.com/atilimcetin/guided-filter/master/guidedfilter.cpp 
    public class GuidedFilter {

        private int width;
        private int height;
        private int colorComponents;

        public GuidedFilter(int width, int height, int colorComponents) {
            this.width = width;
            this.height = height;
            this.colorComponents = colorComponents;
        }
        
        
        public static byte[] RunGuidedFilter(byte[] bytes, byte[] alpha, int i, double eps) {
            throw new NotImplementedException("TODO");
        }
        
        
        private byte[] BoxFilter(byte[] image, int boxSize) {
            return ImageBlur.RunBoxBlur(image, width, height, boxSize, colorComponents);
        }
        
        
        
        public static byte[] ElementwiseMultiply(byte[] arrayA, byte[] arrayB)
        {
            if (arrayA.Length != arrayB.Length)
                throw new ArgumentException("Input arrays must have the same length.");

            byte[] result = new byte[arrayA.Length];

            for (int i = 0; i < arrayA.Length; i++)
            {
                // Convert bytes to integers for intermediate calculations
                int intResult = (int)arrayA[i] * arrayB[i];

                // Check for overflow and clip the result to byte range
                result[i] = (byte)Math.Min(byte.MaxValue, Math.Max(0, intResult));
            }

            return result;
        }
        
        
        
        
        
    }
}