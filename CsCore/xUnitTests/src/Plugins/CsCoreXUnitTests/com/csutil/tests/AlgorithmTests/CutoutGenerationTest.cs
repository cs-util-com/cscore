using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class CutoutGenerationTest {

        public CutoutGenerationTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task CutoutGenerationTest1() {
            var tempFolder = EnvironmentV2.instance.GetOrAddTempFolder("CutoutGenerationTest1");
            var inputImage = await MyImageFileRef.DownloadFileIfNeeded(tempFolder, "https://raw.githubusercontent.com/cs-util-com/cscore/update/release_1_10_prep/CsCore/assets/16999.jpg");

            // Generate a cutout of the input image:
            var cutoutResult = GenerateCutOut.Generate(inputImage, 240, 5, 10, 1e-5, 140);

            await using var targetStream = tempFolder.GetChild("Cutout.png").OpenOrCreateForReadWrite();
            var flipped = ImageUtility.FlipImageVertically(cutoutResult, inputImage.Width, inputImage.Height, (int)inputImage.ColorComponents);
            new ImageWriter().WritePng(flipped, inputImage.Width, inputImage.Height, ColorComponents.RedGreenBlueAlpha, targetStream);
        }

    }

}