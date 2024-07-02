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

            var boxFilterResult = image.RunBoxFilter(BoxFilterRadius);

            var outputFile = tempFolder.GetChild("BoxFilter" + (BoxFilterRadius * 2) + ".png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(boxFilterResult, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outputStream);

        }

    }

}