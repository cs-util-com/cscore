using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {

    public class FilterTest {

        public FilterTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private const int Radius = 21;

        [Fact]
        public async Task BoxFilter4ChannelTest() {

            var tempFolder = EnvironmentV2.instance.GetOrAddTempFolder("BoxFilter4ChannelTest");
            var inputImage = await AlgorithmTests.MyImageFileRef.LoadImage(tempFolder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");


            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = Filter.BoxFilter(image.Data, image.Width, image.Height, Radius, (int)image.ColorComponents);
            var test = folder.GetChild("BoxFilter" + Radius * 2 + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
                writer.WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        [Fact]
        public async Task OldBoxFilterByteTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("OldBoxFilterByteTest");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = ImageBlur.RunBoxBlur(image.Data, image.Width, image.Height, Radius, (int)image.ColorComponents);
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("OldBoxFilterByte" + Radius + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        [Fact]
        public async Task OldBoxFilterDoubleFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("OldBoxFilterDoubleFilterTest");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var doubleIm = GuidedFilter.ConvertToDouble(image.Data);
            var imageResult = ImageBlur.RunBoxBlurDouble(doubleIm, image.Width, image.Height, 21, (int)image.ColorComponents);
            var byteIm = GuidedFilter.ConvertToByte(imageResult);
            var flippedResult = ImageUtility.FlipImageVertically(byteIm, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("OldBoxfilterDouble" + Radius + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        private static async Task DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyImageFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }

    }

}