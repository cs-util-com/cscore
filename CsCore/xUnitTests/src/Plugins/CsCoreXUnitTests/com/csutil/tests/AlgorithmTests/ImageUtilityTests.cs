using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.csutil;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using StbImageWriteSharp;
using Xunit;
using Zio;
using ColorComponents = StbImageSharp.ColorComponents;

namespace com.csutil.tests.AlgorithmTests {
    public class ImageUtilityTests {
        public ImageUtilityTests(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task RunUtility_ShouldApplyBlurCorrectly() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("RunUtility_ShouldApplyBlurCorrectly");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var flippedResult = ImageUtility.FlipImageVertically(image.Data, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("FlipVertically.png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            var horizontalflip = ImageUtility.FlipImageHorizontally(image.Data, image.Width, image.Height, (int)(image.ColorComponents));
            var test2 = folder.GetChild("FlipHorizontal.png");
            {
                using var stream = test2.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(horizontalflip, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            //Cropping works just keeping in mind that imageLoader flips the image beforehand so if you want to crop bottom right you need to crop now at top left
            var cropImage = ImageUtility.CroppingImage(image.Data, image.Width, image.Height, (int)(image.ColorComponents), 0, 0, image.Width - 300, image.Height - 300);
            var test3 = folder.GetChild("CropImage.png");
            {
                using var stream = test3.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(cropImage, image.Width - 300, image.Height - 300, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            var resizedImage = ImageUtility.ResizeImage(image.Data, image.Width, image.Height, (int)(image.ColorComponents), image.Width + 400, image.Height + 400);
            var test4 = folder.GetChild("UpsizedImage.png");
            {
                using var stream = test4.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(resizedImage, image.Width + 400, image.Height + 400, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
            var resizedImage2 = ImageUtility.ResizeImage(image.Data, image.Width, image.Height, (int)(image.ColorComponents), image.Width - 400, image.Height - 400);
            var test5 = folder.GetChild("DownsizedImage.png");
            {
                using var stream = test5.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(resizedImage2, image.Width - 400, image.Height - 400, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        private static async Task DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyImageFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }

    }

}