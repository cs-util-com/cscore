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
    }
    
    
    
}