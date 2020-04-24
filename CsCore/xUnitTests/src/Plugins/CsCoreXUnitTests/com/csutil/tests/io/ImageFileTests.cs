using System;
using System.IO;
using System.Threading.Tasks;
using blurhash;
using com.csutil.io;
using StbImageLib;
using Xunit;
using Zio;

namespace com.csutil.tests {

    public class ImageFileTests {

        public ImageFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var t = Log.MethodEntered();
            FileEntry imgFile = await GetImageFile("testImage1.jpg", 50, 50);
            ImageResult image = await ImageLoader.LoadImageInBackground(imgFile);
            Assert.True(image.Height > 0);
            Assert.True(image.Width > 0);
            Log.MethodDone(t);
            Log.d("Image " + imgFile + " has size: " + image.Width + " x " + image.Height);
        }

        private static async Task<FileEntry> GetImageFile(string imageFileName, int width, int height) {
            FileEntry imgFile = EnvironmentV2.instance.GetCurrentDirectory().GetChild(imageFileName);
            if (!imgFile.Exists) { // If the file does not exist, download a random image and save it there:
                var stream = await new Uri($"https://picsum.photos/{width}/{height}").SendGET().GetResult<Stream>();
                imgFile.SaveStream(stream);
                stream.Dispose();
                Log.e("Saved a random image for testing to " + imgFile.GetFullFileSystemPath());
            }
            return imgFile;
        }

        [Fact]
        public async Task BlurHashTest() {
            var t = Log.MethodEntered();
            FileEntry imgFile = await GetImageFile("testImage2.jpg", 50, 50);
            ImageResult image = await ImageLoader.LoadImageInBackground(imgFile);

            var t1 = Log.MethodEntered("Blurhash.Encode");
            string hash = Blurhash.Encoder.Encode(image.ToPixels(), 2, 2);
            Log.MethodDone(t1);

            var t2 = Log.MethodEntered("Blurhash.Decode");
            var imgFromHash = Blurhash.Decoder.Decode(hash, image.Width, image.Height);
            Log.MethodDone(t2);

            Assert.NotEmpty(imgFromHash);
        }



    }

}