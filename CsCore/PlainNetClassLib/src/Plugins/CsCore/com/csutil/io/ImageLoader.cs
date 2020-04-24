using blurhash;
using StbImageLib;
using System.IO;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.io {

    public static class ImageLoader {

        public static Task<ImageResult> LoadImageInBackground(FileEntry imgFile) {
            return TaskV2.Run(() => LoadAndDispose(imgFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)));
        }

        public static Task<ImageResult> LoadImageInBackground(byte[] bytes) {
            return TaskV2.Run(() => LoadAndDispose(new MemoryStream(bytes)));
        }

        public static Task<ImageResult> LoadAndDispose(Stream stream) {
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            stream.Dispose();
            Conversion.stbi__vertical_flip(image.Data, image.Width, image.Height, 4);
            return Task.FromResult(image);
        }

        public static Pixel[,] ToPixels(this ImageResult self) {
            var bytesPerChan = self.BitsPerChannel == 8 ? 1 : 2;
            return ToPixels(self.Data, self.Width, self.Height, bytesPerChan);
        }

        private static Pixel[,] ToPixels(byte[] data, int w, int h, int bytesPerChan) {
            var pixels = new Pixel[w, h];
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    pixels[x, y] = ToPixel(data, x, y, bytesPerChan);
                }
            }
            return pixels;
        }

        private static Pixel ToPixel(byte[] data, int x, int y, int bytesPerChan) {
            var start = x * y;
            return new Pixel() {
                R = ToColorLittleEndian(data, start, bytesPerChan),
                G = ToColorLittleEndian(data, start + bytesPerChan, bytesPerChan),
                B = ToColorLittleEndian(data, start + bytesPerChan + bytesPerChan, bytesPerChan)
            };

        }

        private static double ToColorLittleEndian(byte[] data, int start, int bytesPerChan) {
            if (bytesPerChan == 1) { return data[start]; }
            return data[start + 1] << 8 + data[start];
        }

    }

}
