using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageWriteSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zio;

namespace com.csutil.tests.AlgorithmTests
{
    public class FilterTest
    {
        public FilterTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        private const int Radius = 21;
        
        [Fact]
        public async Task BoxFilter4ChannelTest()
        {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            
            
            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = Filter.BoxFilter(image.Data, image.Width, image.Height, Radius, (int)image.ColorComponents);
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("BoxFilter" + Radius*2+".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }
        
        [Fact]
        public async Task OldBoxFilterByteTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = ImageBlur.RunBoxBlur(image.Data, image.Width, image.Height, Radius, (int)image.ColorComponents);
            var flippedResult = ImageUtility.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("OldBoxFilterByte" + Radius +".png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }

        
        [Fact]
        public async Task OldBoxFilterDoubleFilterTest() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("FilterTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var doubleIm = GuidedFilter.ConvertToDouble(image.Data);
            var imageResult = ImageBlur.RunBoxBlurDouble(doubleIm, image.Width, image.Height, 21, (int)image.ColorComponents);
            var byteIm = GuidedFilter.ConvertToByte(imageResult);
            var flippedResult = ImageUtility.FlipImageVertically(byteIm, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("OldBoxfilterDouble" + Radius + ".png");
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





