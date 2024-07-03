using System.Threading.Tasks;
using com.csutil.algorithms.images;
using StbImageWriteSharp;
using Xunit;

namespace com.csutil.tests.AlgorithmTests {

    public class ImageUtilityTests {

        public ImageUtilityTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task TestImageUtility1() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("RunUtility_ShouldApplyBlurCorrectly");

            var inputImage = folder.GetChild("GT04-image.png");
            var image = await MyImageFileRef.DownloadFileIfNeeded(inputImage, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var flippedResult = image.FlipImageVertically();
            {
                var test = folder.GetChild("FlipVertically.png");
                using var stream = test.OpenOrCreateForWrite();
                new ImageWriter().WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            var horizontalflip = image.FlipImageHorizontally();
            {
                var test2 = folder.GetChild("FlipHorizontal.png");
                using var stream = test2.OpenOrCreateForWrite();
                new ImageWriter().WritePng(horizontalflip, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            //Cropping works just keeping in mind that imageLoader flips the image beforehand so if you want to crop bottom right you need to crop now at top left
            var cropImage = image.CroppingImage(0, 0, image.Width - 300, image.Height - 300);
            {
                var test3 = folder.GetChild("CropImage.png");
                using var stream = test3.OpenOrCreateForWrite();
                new ImageWriter().WritePng(cropImage, image.Width - 300, image.Height - 300, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            var resizedImage = image.ResizeImage(image.Width + 400, image.Height + 400);
            {
                var test4 = folder.GetChild("UpsizedImage.png");
                using var stream = test4.OpenOrCreateForWrite();
                new ImageWriter().WritePng(resizedImage, image.Width + 400, image.Height + 400, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

            var resizedImage2 = image.ResizeImage(image.Width - 400, image.Height - 400);
            {
                var test5 = folder.GetChild("DownsizedImage.png");
                using var stream = test5.OpenOrCreateForWrite();
                new ImageWriter().WritePng(resizedImage2, image.Width - 400, image.Height - 400, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }

        }

    }

}