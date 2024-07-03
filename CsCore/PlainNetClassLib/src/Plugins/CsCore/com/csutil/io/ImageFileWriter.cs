using System.IO;
using com.csutil.algorithms.images;
using StbImageSharp;
using StbImageWriteSharp;

namespace com.csutil.io {

    public static class ImageFileWriter {

        public static void WritePngToStream(this ImageResult sourceImage, Stream targetStream) {
            var verticallyFlipped = sourceImage.FlipImageVertically();
            new ImageWriter().WritePng(verticallyFlipped, sourceImage.Width, sourceImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, targetStream);
        }

        public static void WriteJpgToStream(this ImageResult sourceImage, Stream targetStream, int quality) {
            var verticallyFlipped = sourceImage.FlipImageVertically();
            new ImageWriter().WriteJpg(verticallyFlipped, sourceImage.Width, sourceImage.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, targetStream, quality);
        }

    }

}