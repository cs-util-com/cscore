using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class TrimapGenerationTest {

        public TrimapGenerationTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestTrimapGeneration() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("TestTrimapGeneration");
            var image = await MyImageFileRef.DownloadFileIfNeeded(folder, "https://raw.githubusercontent.com/cs-util-com/cscore/update/release_1_10_prep/CsCore/assets/16999.jpg");

            var width = image.Width;
            var height = image.Height;
            var kernel = 1;

            var floodFilled = image.RunFloodFillAlgorithm(240);
            {
                var test = folder.GetChild("FloodFilled.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(floodFilled, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var dilated = Filter.Dilate(floodFilled, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("Dilated.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(dilated, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var erodedFill = Filter.Erode(floodFilled, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("Eroded.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(erodedFill, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var trimap = TrimapGeneration.FromFloodFill(floodFilled, width, height, 4, kernel);
            {
                var trimapPng = folder.GetChild("Trimap.png");
                await using var stream = trimapPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(trimap, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            Log.d("TrimapGenerationTest done, see the results in the temp folder: " + folder.GetFullFileSystemPath());

        }

    }

}