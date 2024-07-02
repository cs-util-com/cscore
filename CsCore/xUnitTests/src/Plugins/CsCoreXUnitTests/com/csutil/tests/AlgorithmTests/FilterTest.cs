using System;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using System.Threading.Tasks;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class FilterTest {

        public FilterTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        const int BoxFilterRadius = 21;

        [Fact]
        public async Task BoxFilter4ChannelTest() {

            var tempFolder = EnvironmentV2.instance.GetOrAddTempFolder("BoxFilter4ChannelTest");
            var image = await AlgorithmTests.MyImageFileRef.DownloadFileIfNeeded(tempFolder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            byte[] boxFilterResult = Filter.BoxFilter(image.Data, image.Width, image.Height, BoxFilterRadius, (int)image.ColorComponents);

            var outputFile = tempFolder.GetChild("BoxFilter" + BoxFilterRadius * 2 + ".png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(boxFilterResult, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outputStream);

        }

        // [Fact]
        [Obsolete]
        public async Task OldBoxFilterByteTest() {

            var tempFolder = EnvironmentV2.instance.GetOrAddTempFolder("OldBoxFilterByteTest");

            var imageFile = tempFolder.GetChild("GT04-image.png");
            var image = await MyImageFileRef.DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            byte[] boxBlurResult = ImageBlur.RunBoxBlur(image.Data, image.Width, image.Height, BoxFilterRadius, (int)image.ColorComponents);
            
            var outputFile = tempFolder.GetChild("OldBoxFilterByte" + BoxFilterRadius + ".png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(boxBlurResult, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, outputStream);

        }

        // [Fact]
        [Obsolete]
        public async Task OldBoxFilterDoubleFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("OldBoxFilterDoubleFilterTest");

            var imageFile = folder.GetChild("GT04-image.png");
            var image = await MyImageFileRef.DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var boxBlurInput = GuidedFilter.ConvertToDouble(image.Data);
            double[] boxBlurResult = ImageBlur.RunBoxBlurDouble(boxBlurInput, image.Width, image.Height, 21, (int)image.ColorComponents);
            
            var outputFile = folder.GetChild("OldBoxfilterDouble" + BoxFilterRadius + ".png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var byteIm = GuidedFilter.ConvertToByte(boxBlurResult);
            var flippedResult = ImageUtility.FlipImageVertically(byteIm, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, outputStream);

        }

    }

}