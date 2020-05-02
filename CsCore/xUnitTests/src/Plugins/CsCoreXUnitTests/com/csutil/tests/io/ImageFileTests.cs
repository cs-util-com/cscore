using System;
using System.IO;
using System.Threading.Tasks;
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
            FileEntry imgFile = await GetImageFile("testImage1.jpg");
            ImageResult image = await ImageLoader.LoadImageInBackground(imgFile);
            Assert.True(image.Height > 0);
            Assert.True(image.Width > 0);
            Log.MethodDone(t);
            Log.d("Image " + imgFile + " has size: " + image.Width + " x " + image.Height);
        }

        private static async Task<FileEntry> GetImageFile(string imageFileName) {
            FileEntry imgFile = EnvironmentV2.instance.GetCurrentDirectory().GetChild(imageFileName);
            if (!imgFile.Exists) { // If the file does not exist, download a random image and save it there:
                Log.d("Saving random image for testing to: " + imgFile.GetFullFileSystemPath());
                var stream = await new Uri("https://picsum.photos/50/50").SendGET().GetResult<Stream>();
                imgFile.SaveStream(stream);
                stream.Dispose();
            }
            return imgFile;
        }

    }

}