using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using com.csutil.algorithms.images;
using com.csutil.io;
using com.csutil.model;
using StbImageSharp;
using StbImageWriteSharp;
using Xunit;
using Zio;
using ColorComponents = StbImageSharp.ColorComponents;


namespace com.csutil.tests.AlgorithmTests {
    public class ImageBlurTest {
        public ImageBlurTest(Xunit.Abstractions.ITestOutputHelper logger) { logger.UseAsLoggingOutput(); }

        [Fact]
        public async Task RunBoxBlur_ShouldApplyBlurCorrectly() {

            var folder = EnvironmentV2.instance.GetOrAddAppDataFolder("BlurTesting");

            var imageFile = folder.GetChild("GT04-image.png");
            await DownloadFileIfNeeded(imageFile, "http://atilimcetin.com/global-matting/GT04-image.png");

            var image = await ImageLoader.LoadImageInBackground(imageFile);
            var imageResult = ImageBlur.RunBoxBlur(image.Data, image.Width, image.Height, 21, (int)image.ColorComponents);
            var flippedResult = ImageFlip.FlipImageVertically(imageResult, image.Width, image.Height, (int)image.ColorComponents);
            var test = folder.GetChild("Blurred.png");
            {
                using var stream = test.OpenOrCreateForWrite();
                ImageWriter writer = new ImageWriter();
                writer.WritePng(flippedResult, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
            }
        }


        [Fact]
        public async Task BitWiseByteArrayMultTst() {

            var test1 = new byte[] {1,2,3};
            var test2 = new byte[] {5, 6, 7};

            var test3 = GuidedFilter.ElementwiseMultiply(test1, test2);
            var test = 2;
        }
        
        
        
        
        
        
        
        
        private static async Task DownloadFileIfNeeded(FileEntry self, string url) {
            var imgFileRef = new MyFileRef() { url = url, fileName = self.Name };
            await imgFileRef.DownloadTo(self.Parent, useAutoCachedFileRef: true);
        }
        private class MyFileRef : IFileRef {
            public string dir { get; set; }
            public string fileName { get; set; }
            public string url { get; set; }
            public Dictionary<string, object> checksums { get; set; }
            public string mimeType { get; set; }
        }
    }

}