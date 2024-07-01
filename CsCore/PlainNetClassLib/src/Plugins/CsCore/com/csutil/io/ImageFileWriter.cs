using System.IO;
using StbImageSharp;
using StbImageWriteSharp;

namespace com.csutil.io {

    public static class ImageFileWriter {

        public static void WritePngToStream(this ImageResult sourceImage, Stream targetStream) {
            ImageWriter writer = new ImageWriter();
            var horizontallyFlipped = FlipImageHorizontally(sourceImage.Data, sourceImage.Width, sourceImage.Height, sourceImage.Data.Length);
            writer.WritePng(horizontallyFlipped, sourceImage.Width, sourceImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, targetStream);
        }
        
        public static void WriteJpgToStream(this ImageResult sourceImage, Stream targetStream, int quality) {
            ImageWriter writer = new ImageWriter();
            var horizontallyFlipped = FlipImageHorizontally(sourceImage.Data, sourceImage.Width, sourceImage.Height, sourceImage.Data.Length);
            writer.WriteJpg(horizontallyFlipped, sourceImage.Width, sourceImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, targetStream, quality);
        }

        public static byte[] FlipImageHorizontally(byte[] img, int width, int height, int length) {
            byte[] data = new byte[length];
            for (int i = 0; i < height; i++) {
                for (int j = 0; j < width * 4; j++) {
                    data[(height - 1 - i) * width * 4 + j] = img[i * width * 4 + j];
                }
            }
            return data;
        }

    }

}