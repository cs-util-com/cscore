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

namespace com.csutil.tests.AlgorithmTests
{
    public class FloodFillVsFloodFillOnMat
    {
        public FloodFillVsFloodFillOnMat(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }


        [Fact]
        public async Task FloodFillTest()
        {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FloodFilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");
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
        public async Task FloodFillOnMatTest()
        {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FloodFillonMatTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var mat = new Mat<byte>(image.Width, image.Height, (int)image.ColorComponents, image.Data);
            var time = Stopwatch.StartNew();
            var imageResult = FloodFillMat.FloodFillAlgorithm(mat, 50);
            time.Stop();
            var stoptime = time.ElapsedMilliseconds;
            var flippedResult = ImageUtility.FlipImageVertically(imageResult.data, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("FloodMat" + stoptime + ".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }



        private static async Task DownloadFileIfNeeded(FileEntry self, string url)
        {
            var imgFileRef = new MyFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }
        private class MyFileRef : IFileRef
        {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }
    }
}
