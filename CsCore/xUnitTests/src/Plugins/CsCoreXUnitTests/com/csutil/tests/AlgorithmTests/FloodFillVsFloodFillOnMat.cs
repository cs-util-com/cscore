using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests {
    public class FloodFillVsFloodFillOnMat {
        public FloodFillVsFloodFillOnMat(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task FloodFillTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("FloodFillTest");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var time = Stopwatch.StartNew();
            var imageResult = FloodFill.FloodFillAlgorithm(image, 50);
            time.Stop();
            var timestop = time.ElapsedMilliseconds;
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("FloodFill" + timestop + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        [Fact]
        public async Task FloodFillOnMatTest() {

            var folder = EnvironmentV2.instance.GetOrAddTempFolder("FloodFillOnMatTest");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "https://raw.githubusercontent.com/cs-util/global-matting/master/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var mat = new Mat<byte>(image.Width, image.Height, (int)image.ColorComponents, image.Data);
            var time = Stopwatch.StartNew();
            var imageResult = FloodFillMat.FloodFillAlgorithm(mat, 50);
            time.Stop();
            var stoptime = time.ElapsedMilliseconds;
            var flippedResult = ImageUtility.FlipImageVertically(imageResult.data, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("FloodMat" + stoptime + ".png");

            using var stream = test.OpenOrCreateForWrite();
            ImageWriter writer = new ImageWriter();
            writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
        }

        private static async Task DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyImageFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }

    }

}