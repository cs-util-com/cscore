using StbImageLib;
using System.IO;
using System.Threading.Tasks;
using Zio;

namespace com.csutil.io {

    public static class ImageLoader {

        public static Task<ImageResult> LoadImageInBackground(FileEntry imgFile) {
            AssertV2.IsTrue(imgFile.Exists, "!imgFile.Exists: " + imgFile);
            return TaskV2.Run(() => LoadAndDispose(imgFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read)));
        }

        public static Task<ImageResult> LoadImageInBackground(byte[] bytes) {
            return TaskV2.Run(() => LoadAndDispose(new MemoryStream(bytes)));
        }

        public static async Task<ImageResult> LoadAndDispose(Stream stream) {
            stream = await stream.CopyToSeekableStreamIfNeeded(disposeOriginalStream: true);
            AssertV2.AreNotEqual(0, stream.Length, "LoadAndDispose: stream.Length");
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            stream.Dispose();
            Conversion.stbi__vertical_flip(image.Data, image.Width, image.Height, 4);
            return image;
        }

        

    }

}
