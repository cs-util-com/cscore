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

        public static double Compare(this MagickImage self, MagickImage newImg, out MagickImage diffImg, ErrorMetric errorMetric = ErrorMetric.MeanSquared) {
            diffImg = new MagickImage();
            return self.Compare(newImg, new CompareSettings() { Metric = errorMetric }, diffImg);
        }

        public static void Compare(this MagickImage self, FileEntry imgFile, double MAX_ALLOWED_DIFF = 0.001) {
            using (MagickImage oldImg = new MagickImage()) {
                oldImg.LoadFromFileEntry(imgFile);
                FileEntry diffFile = imgFile.Parent.GetChild(imgFile.NameWithoutExtension + "_diff" + imgFile.ExtensionWithDot);
                if (oldImg.Compare(self, out MagickImage imgWithDifferences) > MAX_ALLOWED_DIFF) {
                    Log.e("Detected high difference vs old image, see diffFile: " + diffFile);
                } else {
                    self.SaveToFileEntry(imgFile);
                }
                imgWithDifferences.SaveToFileEntry(diffFile);
                imgWithDifferences.Dispose();
            }
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
                newImg.Compare(imFileToDiff_original);
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