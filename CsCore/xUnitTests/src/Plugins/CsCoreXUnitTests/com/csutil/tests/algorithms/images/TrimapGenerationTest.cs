using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests.images {

    public class TrimapGenerationTest {

        public TrimapGenerationTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestFloodFillVsColorCheckAlgo() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("TestFloodFillVsColorCheckAlgo");
            var imgFile = folder.GetChild("0_inputImage.jpg");
            var image = await MyImageFileRef.DownloadFileIfNeeded(imgFile, "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/16999.jpg");

            var width = image.Width;
            var height = image.Height;

            var time1 = Log.MethodEntered("RunColorCheckAlgorithm");
            var colorChecked = image.RunColorCheckAlgorithm();
            Log.MethodDone(time1);
            {
                var test = folder.GetChild("2_ColorChecked.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(colorChecked, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var time2 = Log.MethodEntered("RunFloodFill");
            var floodFilled = image.RunFloodFill();
            Log.MethodDone(time2);
            {
                var test = folder.GetChild("1_FloodFilled.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(floodFilled, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

        }

        [Fact]
        public async Task TestTrimapGeneration() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("TestTrimapGeneration");
            var imgFile = folder.GetChild("0_inputImage.jpg");
            var image = await MyImageFileRef.DownloadFileIfNeeded(imgFile, "https://raw.githubusercontent.com/cs-util-com/cscore/master/CsCore/assets/16999.jpg");

            var width = image.Width;
            var height = image.Height;
            var kernel = 2;

            var floodFilled = image.RunColorCheckAlgorithm(240);
            {
                var test = folder.GetChild("1_FloodFilled.png");
                await using var stream = test.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(floodFilled, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var dilated = Filter.Dilate(floodFilled, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("4_Dilated.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(dilated, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var eroded = Filter.Erode(floodFilled, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("6_Eroded.png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(eroded, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var closed = Filter.Erode(dilated, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("3_Closed (Dilated then Eroded).png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(closed, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var opened = Filter.Dilate(eroded, width, height, 4, kernel);
            {
                var dilationPng = folder.GetChild("5_Opened (Eroded then Dilated).png");
                await using var stream = dilationPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(opened, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var trimapV1 = TrimapGeneration.FromFloodFill(floodFilled, width, height, 4, kernel);
            {
                var trimapPng = folder.GetChild("2_1_Trimap.png");
                await using var stream = trimapPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(trimapV1, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            var trimapVariant2 = TrimapGeneration.FromClosedFloodFill(floodFilled, width, height, 4, kernel);
            {
                var trimapPng = folder.GetChild("2_2_Trimap (Closed Flood Fill).png");
                await using var stream = trimapPng.OpenOrCreateForReadWrite();
                var flipped = ImageUtility.FlipImageVertically(trimapVariant2, width, height, (int)image.ColorComponents);
                new ImageWriter().WritePng(flipped, width, height, ColorComponents.RedGreenBlueAlpha, stream);
            }

            Log.d("TrimapGenerationTest done, see the results in the temp folder: " + folder.GetFullFileSystemPath());

        }

    }

}