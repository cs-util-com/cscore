using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class FloodFillVsFloodFillOnMat {

        public FloodFillVsFloodFillOnMat(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task FloodFillTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("FloodFillVsFloodFillOnMat");
            var image = await MyImageFileRef.DownloadFileIfNeeded(folder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            byte[] imageResult = FloodFill.FloodFillAlgorithm(image, 50);

            var outputFile = folder.GetChild("FloodFillResult.png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outputStream);
            
        }

        [Fact]
        public async Task FloodFillOnMatTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("FloodFillVsFloodFillOnMat");
            var image = await MyImageFileRef.DownloadFileIfNeeded(folder, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            Mat<byte> inputMat = new Mat<byte>(image.Width, image.Height, (int)image.ColorComponents, image.Data);
            Mat<byte> imageResultMat = FloodFillMat.FloodFillAlgorithm(inputMat, 50);

            var outputFile = folder.GetChild("FloodFillOnMatResult.png");
            using var outputStream = outputFile.OpenOrCreateForWrite();
            var flippedResult = ImageUtility.FlipImageVertically(imageResultMat.data, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flippedResult, image.Width, image.Height, ColorComponents.RedGreenBlueAlpha, outputStream);
            
        }

    }

}