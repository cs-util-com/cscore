using System;
using System.IO;
using System.Threading.Tasks;
using com.csutil.integrationTests.http;
using com.csutil.io;
using Xunit;
using Zio;

namespace com.csutil.integrationTests {

    public class ImageFileTests {

        public ImageFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            var t = Log.MethodEntered();
            GetTempFolder().DeleteV2();
            FileEntry imgFile = await GetImageFile("testImage1.jpg");
            var image = await ImageLoader.LoadImageInBackground(imgFile);
            Assert.True(image.Height > 0);
            Assert.True(image.Width > 0);
            Log.MethodDone(t);
            Log.d("Image " + imgFile + " has size: " + image.Width + " x " + image.Height);
        }

        private static async Task<FileEntry> GetImageFile(string imageFileName) {
            FileEntry imgFile = GetTempFolder().GetChild(imageFileName);
            // If the file does not exist or is invalid, download a random image and save it there:
            if (!imgFile.Exists || imgFile.GetFileSize() == 0) {
                Log.d("Saving random image for testing to: " + imgFile.GetFullFileSystemPath());
                var stream = await new Uri(RestTests.IMG_PLACEHOLD_SERVICE_URL+"/50/50").SendGET().GetResult<Stream>();
                Assert.NotNull(stream);
                imgFile.SaveStream(stream, resetStreamToStart: false);
                stream.Dispose();
            }
            Assert.True(imgFile.Exists);
            Assert.NotEqual(0, imgFile.GetFileSize());
            return imgFile;
        }

        private static DirectoryEntry GetTempFolder() { return EnvironmentV2.instance.GetOrAddTempFolder("ImageFileTests"); }

    }

}