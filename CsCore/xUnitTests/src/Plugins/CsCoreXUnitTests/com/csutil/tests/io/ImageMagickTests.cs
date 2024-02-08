#if ENABLE_IMAGE_MAGICK

using System;
using System.IO;
using System.Threading.Tasks;
using ImageMagick;
using Xunit;
using Zio;
using com.csutil.integrationTests.http;

namespace com.csutil.tests {

    public class ImageMagickTests {

        public ImageMagickTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task ExampleUsage1() {
            GetTempFolder().DeleteV2();
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

        [Fact]
        public async Task TestImgDiffing() {
            FileEntry imFileToDiff_original = await GetImageFile("imFileToDiff_original.jpg");
            FileEntry imFileToDiff_modified = imFileToDiff_original.Parent.GetChild("imFileToDiff_sharpened.jpg");

            var t = Log.MethodEntered("Load modify save compare save diff");

            if (!imFileToDiff_modified.IsNotNullAndExists()) {
                GenerateModifiedVariant(imFileToDiff_original, imFileToDiff_modified);
            }

            using (MagickImage newImg = new MagickImage()) {
                newImg.LoadFromFileEntry(imFileToDiff_modified);
                var diff = newImg.Compare(imFileToDiff_original, ErrorMetric.MeanSquared, 0.000001);
                Assert.NotNull(diff);
            }

            Log.MethodDone(t);
            Log.d("Saved to " + imFileToDiff_modified.GetFullFileSystemPath());
        }

        private static void GenerateModifiedVariant(FileEntry imFileToDiff_original, FileEntry imFileToDiff_sharpened) {
            using (MagickImage original = new MagickImage()) {
                original.LoadFromFileEntry(imFileToDiff_original);
                using (MagickImage sharpened = new MagickImage(original)) {
                    sharpened.Sharpen(); // Create a sharpened version of the original image
                    sharpened.SaveToFileEntry(imFileToDiff_sharpened);
                }
            }
        }

        private static async Task<FileEntry> GetImageFile(string imageFileName) {
            FileEntry imgFile = GetTempFolder().GetChild(imageFileName);
            // If the file does not exist or is invalid, download a random image and save it there:
            if (!imgFile.Exists || imgFile.GetFileSize() == 0) {
                Log.d("Saving random image for testing to: " + imgFile.GetFullFileSystemPath());
                var stream = await new Uri(RestTests.IMG_PLACEHOLD_SERVICE_URL + "/4000/4000").SendGET().GetResult<Stream>();
                stream = await stream.CopyToSeekableStreamIfNeeded(true);
                imgFile.SaveStream(stream);
                stream.Dispose();
            }
            Assert.True(imgFile.Exists);
            Assert.NotEqual(0, imgFile.GetFileSize());
            return imgFile;
        }

        private static DirectoryEntry GetTempFolder() { return EnvironmentV2.instance.GetOrAddTempFolder("ImageMagickTests"); }

    }

}

#endif