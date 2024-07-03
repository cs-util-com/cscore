using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageSharp;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class ImageAlphaMattingTests {

        public ImageAlphaMattingTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        /// <summary> Ported example usage from https://github.com/atilimcetin/global-matting/tree/master#code  </summary>
        [Fact]
        public async Task TestGlobalMatting() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("TestGlobalMatting");

            var imageFile = folder.GetChild("GT04-image.png");
            var image = await MyImageFileRef.DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var trimapFile = folder.GetChild("GT04-trimap.png");
            var trimap = await MyImageFileRef.DownloadFileIfNeeded(trimapFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-trimap.png");

            var resultOfOriginalCppImplementation = folder.GetChild("GT04-alpha (result of original Cpp implementation).png");
            await MyImageFileRef.DownloadFileIfNeeded(resultOfOriginalCppImplementation, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-alpha.png");

            var trimapBytes = trimap.Data;
            var imageMatting = new GlobalMatting(image.Data.DeepCopy(), image.Width, image.Height, (int)image.ColorComponents);
            imageMatting.ExpansionOfKnownRegions(ref trimapBytes, niter: 9);
            imageMatting.RunGlobalMatting(trimapBytes, out var foreground, out var alphaData, out var conf);
            // filter the result with fast guided filter
            var alphaDataGuided = imageMatting.RunGuidedFilter(alphaData, r: 10, eps: 1e-5);

            var alpha = new ImageResult {
                Width = image.Width,
                Height = image.Height,
                SourceComponents = image.ColorComponents,
                ColorComponents = image.ColorComponents,
                BitsPerChannel = image.BitsPerChannel,
                Data = alphaDataGuided
            };

            for (int x = 0; x < trimap.Width; ++x) {
                for (int y = 0; y < trimap.Height; ++y) {
                    if (trimap.GetPixel(x, y).R == 0) {
                        alpha.SetPixel(x, y, new Pixel(0, 0, 0, 255));
                    } else if (trimap.GetPixel(x, y).R == 255) {
                        alpha.SetPixel(x, y, new Pixel(255, 255, 255, 255));
                    }
                }
            }

            var finalAlphaFile = folder.GetChild("FinalAlphaResult.png");
            await using var stream = finalAlphaFile.OpenOrCreateForReadWrite();
            var flipped = ImageUtility.FlipImageVertically(alpha.Data, image.Width, image.Height, (int)image.ColorComponents);
            new ImageWriter().WritePng(flipped, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);

        }

    }

}