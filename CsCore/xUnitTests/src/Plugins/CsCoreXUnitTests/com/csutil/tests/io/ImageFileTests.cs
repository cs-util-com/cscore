using System.IO;
using StbImageLib;
using Xunit;

namespace com.csutil.tests {

    public class ImageFileTests {

        public ImageFileTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public void ExampleUsage1() {
            var t = Log.MethodEntered();
            var dir = EnvironmentV2.instance.GetCurrentDirectory();
            ImageResult image = LoadImage(dir.GetChild("testImage.jpg"), ColorComponents.RedGreenBlue);
            Assert.True(image.Height > 0, "image.Height=" + image.Height);
            Assert.True(image.Width > 0, "image.Width=" + image.Width);
            Log.MethodDone(t);
            Log.d("Image size: " + image.Width + " x " + image.Height);
        }

        private static ImageResult LoadImage(FileInfo self, ColorComponents? requiredComponents = null) {
            if (!self.ExistsV2()) { throw Log.e("No image found at " + self); }
            var stream = File.OpenRead(self.FullName);
            var image = ImageResult.FromStream(stream, requiredComponents);
            stream.Dispose();
            return image;
        }

    }

}