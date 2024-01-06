using System;
using System.Collections.Generic;
using System.Text;

namespace com.csutil.algorithms.images { 
    public class ImageFlip {
        public static byte[] FlipImageHorizontally(byte[] imageData, int width, int height, int bytesPerPixel) {
            var flippedImage = new byte[imageData.Length];

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var originalIndex = (y * width + x) * bytesPerPixel;
                    var flippedIndex = (y * width + (width - 1 - x)) * bytesPerPixel;

                    for (var byteIndex = 0; byteIndex < bytesPerPixel; byteIndex++) {
                        flippedImage[flippedIndex + byteIndex] = imageData[originalIndex + byteIndex];
                    }
                }
            }

            return flippedImage;
        }

        public static byte[] FlipImageVertically(byte[] imageData, int width, int height, int bytesPerPixel) {
            var flippedImage = new byte[imageData.Length];

            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var originalIndex = (y * width + x) * bytesPerPixel;
                    var flippedIndex = ((height - 1 - y) * width + x) * bytesPerPixel;

                    for (var byteIndex = 0; byteIndex < bytesPerPixel; byteIndex++) {
                        flippedImage[flippedIndex + byteIndex] = imageData[originalIndex + byteIndex];
                    }
                }
            }

            return flippedImage;
        }

    }
}
