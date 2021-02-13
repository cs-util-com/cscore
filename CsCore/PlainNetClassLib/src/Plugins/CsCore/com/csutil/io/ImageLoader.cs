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

        public static Task<ImageInfo> ReadImageInfoAsync(Stream imgStream, bool fallbackToFullImageDownload = true, int exifHeaderByteSizeToTryFirst = 1000) {
            return TaskV2.Run(() => Task.FromResult(ReadImageInfo(imgStream, fallbackToFullImageDownload, exifHeaderByteSizeToTryFirst)));
        }

        public static ImageInfo ReadImageInfo(Stream imgStream, bool fallbackToFullImageDownload, int exifHeaderByteSizeToTryFirst) {
            // Extract first bytes to check for Exif header:
            var part1 = CopyParts(imgStream, bytesToCopy: exifHeaderByteSizeToTryFirst);
            using (var fullImage = new MemoryStream()) {
                part1.CopyTo(fullImage); // Make copy of bytes in case exif reader breaks stream
                try { return GetInfoFromExifReader(part1); }
                catch (Exception) {
                    if (!fallbackToFullImageDownload) { throw; }
                }
                // The exif reader failed and destroyed the part1 stream in this process
                // Fill in rest of bytes from original stream to have full image back
                imgStream.CopyTo(fullImage);
                fullImage.Seek(0, SeekOrigin.Begin); // Set curser back to start
                return ImageInfo.FromStream(fullImage).Value;
            }
        }

        private static Stream CopyParts(Stream self, int bytesToCopy, int offset = 0) {
            var destination = new MemoryStream();
            byte[] buffer = new byte[bytesToCopy];
            int numBytes = self.Read(buffer, offset, buffer.Length);
            destination.Write(buffer, offset, numBytes);
            destination.Seek(0, SeekOrigin.Begin);
            return destination;
        }

        private static ImageInfo GetInfoFromExifReader(Stream stream) {
            stream.Seek(0, SeekOrigin.Begin); // Set curser back to start
            var exif = new ExifLib.ExifReader(stream);
            var res = new ImageInfo();

            if (exif.GetTagValue(ExifLib.ExifTags.ImageWidth, out uint w1)) {
                res.Width = (int)w1;
            } else if (exif.GetTagValue(ExifLib.ExifTags.PixelXDimension, out uint w3)) {
                res.Width = (int)w3;
            }

            if (exif.GetTagValue(ExifLib.ExifTags.ImageLength, out uint h1)) {
                res.Height = (int)h1;
            } else if (exif.GetTagValue(ExifLib.ExifTags.PixelYDimension, out uint h3)) {
                res.Height = (int)h3;
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

    }

}
