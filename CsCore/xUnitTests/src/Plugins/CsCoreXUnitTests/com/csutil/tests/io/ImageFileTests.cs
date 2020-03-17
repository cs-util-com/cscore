using System.IO;
using StbImageLib;
using Xunit;
using Zio;

namespace com.csutil.tests {

    public class ImageFileTests {

        public ImageFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered();
            var imgFile = EnvironmentV2.instance.GetCurrentDirectory().GetChild("testImage1.jpg");
            ImageResult image = LoadImage(imgFile);
            Assert.True(image.Height > 0);
            Assert.True(image.Width > 0);
            Log.MethodDone(t);
            Log.d("Image " + imgFile + " has size: " + image.Width + " x " + image.Height);
        }

        private static ImageResult LoadImage(FileEntry imgFile) {
            if (!imgFile.Exists) { throw Log.e("No image found at " + imgFile); }
            var stream = File.OpenRead(imgFile.FullName);
            var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            stream.Dispose();
            return image;
        }

    }

}