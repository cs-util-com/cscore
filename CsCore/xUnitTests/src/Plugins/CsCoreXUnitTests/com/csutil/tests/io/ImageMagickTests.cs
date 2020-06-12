using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Xunit;
using Zio;

namespace com.csutil.tests {

    public static class MagickImageExtensions {
        public static void SaveToFileEntry(this MagickImage self, FileEntry f) {
            using (Stream outStream = f.OpenOrCreateForWrite()) { self.Write(outStream); }
        }

        public static void LoadFromFileEntry(this MagickImage self, FileEntry f) {
            using (Stream inputStream = f.OpenForRead()) { self.Read(inputStream); }
        }
    }

    public class ImageMagickTests {

        public ImageMagickTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            FileEntry inputFile = await GetImageFile("testImage2.jpg");
            FileEntry outputFile = inputFile.Parent.GetChild("testImage2.png");

            var t = Log.MethodEntered("Load downsize sharpen save");
            using (MagickImage image = new MagickImage()) {
                image.LoadFromFileEntry(inputFile);

                image.Resize(256, 0);
                image.Sharpen();

                image.Format = MagickFormat.Png;
                image.SaveToFileEntry(outputFile);
            }
            Log.MethodDone(t);
            Log.d("Saved to " + outputFile.GetFullFileSystemPath());
        }

        private static async Task<FileEntry> GetImageFile(string imageFileName) {
            FileEntry imgFile = EnvironmentV2.instance.GetCurrentDirectory().GetChild(imageFileName);
            if (!imgFile.Exists) { // If the file does not exist, download a random image and save it there:
                Log.d("Saving random image for testing to: " + imgFile.GetFullFileSystemPath());
                var stream = await new Uri("https://picsum.photos/4000/4000").SendGET().GetResult<Stream>();
                imgFile.SaveStream(stream);
                stream.Dispose();
            }
            return imgFile;
        }

    }

}