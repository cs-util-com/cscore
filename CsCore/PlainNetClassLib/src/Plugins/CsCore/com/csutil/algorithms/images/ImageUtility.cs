using StbImageSharp;

namespace com.csutil.algorithms.images {

    public class ImageUtility {

        public static byte[] FlipImageHorizontally(ImageResult sourceImage) {
            return FlipImageHorizontally(sourceImage.Data, sourceImage.Width, sourceImage.Height, (int)sourceImage.ColorComponents);
        }
        
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

        public static byte[] FlipImageVertically(ImageResult sourceImage) {
            return FlipImageVertically(sourceImage.Data, sourceImage.Width, sourceImage.Height, (int)sourceImage.ColorComponents);
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

        public static byte[] FlipImageVerticallyVariant2(byte[] imageData, int width, int height, int bytesPerPixel = 4) {
            byte[] flippedImage = new byte[imageData.Length];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width * bytesPerPixel; x++) {
                    var originalIndex = y * width * bytesPerPixel + x;
                    var flippedIndex = (height - 1 - y) * width * bytesPerPixel + x;
                    flippedImage[flippedIndex] = imageData[originalIndex];
                }
            }
            return flippedImage;
        }

        public static byte[] CroppingImage(byte[] image, int originalWidth, int originalHeight, int bytesPerPixel, int cropX, int cropY, int cropWidth, int cropHeight) {
            var croppedImage = new byte[cropWidth * cropHeight * bytesPerPixel];

            for (var y = 0; y < cropHeight; y++) {
                for (var x = 0; x < cropWidth; x++) {
                    var pixelIndex = (((y + cropY) * originalWidth) + x + cropX) * bytesPerPixel;
                    var croppedPixelIndex = (y * cropWidth + x) * bytesPerPixel;

                    for (int byteIndex = 0; byteIndex < bytesPerPixel; byteIndex++) {
                        croppedImage[croppedPixelIndex + byteIndex] = image[pixelIndex + byteIndex];
                    }
                }
            }

            return croppedImage;
        }

        public static byte[] ResizeImage(byte[] originalImage, int originalWidth, int originalHeight, int bytesPerPixel, int newWidth, int newHeight) {
            byte[] resizedImage = new byte[newWidth * newHeight * bytesPerPixel];

            double xRatio = (double)originalWidth / newWidth;
            double yRatio = (double)originalHeight / newHeight;

            for (int y = 0; y < newHeight; y++) {
                for (int x = 0; x < newWidth; x++) {
                    int newX = (int)(x * xRatio);
                    int newY = (int)(y * yRatio);

                    int originalPixelIndex = (newY * originalWidth + newX) * bytesPerPixel;
                    int resizedPixelIndex = (y * newWidth + x) * bytesPerPixel;

                    for (int byteIndex = 0; byteIndex < bytesPerPixel; byteIndex++) {
                        resizedImage[resizedPixelIndex + byteIndex] = originalImage[originalPixelIndex + byteIndex];
                    }
                }
            }

            return resizedImage;
        }

    }
}