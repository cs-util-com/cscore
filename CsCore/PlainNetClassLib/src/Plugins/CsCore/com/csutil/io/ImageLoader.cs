using StbImageSharp;
using System;
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

        public static async Task<ImageInfo> GetImageInfoFrom(Stream source) {
            source = await source.CopyToSeekableStreamIfNeeded(disposeOriginalStream: true);
            try {
                try {
                    source.Seek(0, SeekOrigin.Begin);
                    return ImageInfo.FromStream(source).Value;
                }
                catch (Exception e) { Log.w("ImageInfo.FromStream failed", e); }
                source.Seek(0, SeekOrigin.Begin);
                var img = ImageResult.FromStream(source);
                return new ImageInfo() { Width = img.Width, Height = img.Height };
            }
            catch (Exception e) { Log.w("ImageResult.FromStream failed", e); }
            source.Seek(0, SeekOrigin.Begin);
            // ExifLib.ExifReader closes the stream on error so has to be tried last
            return GetInfoFromJpgExifReader(source);
        }

        private static ImageInfo GetInfoFromJpgExifReader(Stream stream) {
            stream.Seek(0, SeekOrigin.Begin); // Set curser back to start
            var exif = new ExifLib.ExifReader(stream);
            var res = new ImageInfo();

            if (exif.GetTagValue(ExifLib.ExifTags.ImageWidth, out uint w1)) {
                res.Width = (int)w1;
            } else if (exif.GetTagValue(ExifLib.ExifTags.PixelXDimension, out uint w2)) {
                res.Width = (int)w2;
            }

            if (exif.GetTagValue(ExifLib.ExifTags.ImageLength, out uint h1)) {
                res.Height = (int)h1;
            } else if (exif.GetTagValue(ExifLib.ExifTags.PixelYDimension, out uint h2)) {
                res.Height = (int)h2;
            }

            //if (exif.GetTagValue(ExifLib.ExifTags.BitsPerSample, out uint b1) && b1 > 0) {
            //    res.BitsPerChannel = (int)b1;
            //} else if (exif.GetTagValue(ExifLib.ExifTags.SamplesPerPixel, out uint b2) && b2 > 0) {
            //    res.BitsPerChannel = (int)b2;
            //} else if (exif.GetTagValue(ExifLib.ExifTags.CompressedBitsPerPixel, out uint b3) && b3 > 0) {
            //    res.BitsPerChannel = (int)b3;
            //} else if (exif.GetTagValue(ExifLib.ExifTags.Compression, out uint b4) && b4 > 0) {
            //    res.BitsPerChannel = (int)b4;
            //}
            return res;
        }

        public static async Task<ImageInfo> GetImageInfoFromFirstBytesOf(Stream source, int bytesToCopy = 5000) {
            using (var firstBytes = await CopyFirstBytes(source, bytesToCopy)) {
                return await GetImageInfoFrom(firstBytes);
            }
        }

        public static async Task<Stream> CopyFirstBytes(this Stream self, int bytesToCopy) {
            var destination = new MemoryStream();
            await CopyFirstBytesTo(self, destination, bytesToCopy);
            return destination;
        }

        // From https://stackoverflow.com/a/13022108
        public static async Task CopyFirstBytesTo(this Stream self, Stream destination, int bytesToCopy) {
            byte[] buffer = new byte[bytesToCopy];
            int read;
            while (bytesToCopy > 0 && (read = await self.ReadAsync(buffer, 0, Math.Min(buffer.Length, bytesToCopy))) > 0) {
                await destination.WriteAsync(buffer, 0, read);
                bytesToCopy -= read;
            }
        }

    }

}